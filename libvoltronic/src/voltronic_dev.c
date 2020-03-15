#include "voltronic_dev.h"
#include "voltronic_dev_impl.h"
#include "voltronic_crc.h"
#include "time_util.h"
#include <string.h>
#include <stdlib.h>

#define END_OF_INPUT '\r'
#define END_OF_INPUT_SIZE sizeof(char)
#define NON_DATA_SIZE (sizeof(voltronic_crc_t) + END_OF_INPUT_SIZE)

struct voltronic_dev_struct_t {
  void* impl_ptr;
  const voltronic_dev_read_f read;
  const voltronic_dev_write_f write;
  const voltronic_dev_close_f close;
};

voltronic_dev_t voltronic_dev_create(
    void* impl_ptr,
    const voltronic_dev_read_f read_function,
    const voltronic_dev_write_f write_function,
    const voltronic_dev_close_f close_function) {

  if ((impl_ptr != 0) &&
    (read_function != 0) &&
    (write_function != 0) &&
    (close_function != 0)) {

    SET_ERRNO(0);
    const voltronic_dev_t dev = malloc(sizeof(struct voltronic_dev_struct_t));
    if (dev != 0) {
      const struct voltronic_dev_struct_t dev_struct = {
        impl_ptr,
        read_function,
        write_function,
        close_function };

      memcpy(dev, &dev_struct , sizeof(struct voltronic_dev_struct_t));
    }

    return dev;
  } else {
    SET_ERRNO(EINVAL);
    return 0;
  }
}

int voltronic_dev_read(
    const voltronic_dev_t dev,
    char* buffer,
    const size_t buffer_size,
    const unsigned long timeout_milliseconds) {

  if (dev != 0 && buffer != 0 && buffer_size > 0) {
    const voltronic_dev_read_f read_function = dev->read;
    void* impl_ptr = dev->impl_ptr;

    SET_ERRNO(0);
    return read_function(impl_ptr, buffer, buffer_size, timeout_milliseconds);
  } else {
    SET_ERRNO(EINVAL);
    return -1;
  }
}

int voltronic_dev_write(
    const voltronic_dev_t dev,
    const char* buffer,
    const size_t buffer_size) {

  if (dev != 0 && buffer != 0 && buffer_size > 0) {
    const voltronic_dev_write_f write_function = dev->write;
    void* impl_ptr = dev->impl_ptr;

    SET_ERRNO(0);
    return write_function(impl_ptr, buffer, buffer_size);
  } else {
    SET_ERRNO(EINVAL);
    return -1;
  }
}

int voltronic_dev_close(voltronic_dev_t dev) {
  SET_ERRNO(EINVAL);
  if (dev != 0) {
    const voltronic_dev_close_f close_function = dev->close;
    if (close_function != 0) {
      void* impl_ptr = dev->impl_ptr;
      if (impl_ptr != 0) {
        SET_ERRNO(0);
        const int result = close_function(impl_ptr);
        if (result > 0) {
          dev->impl_ptr = 0;
          free(dev);

          return result;
        }
      }
    }
  }

  return -1;
}

static int voltronic_read_data_loop(
    const voltronic_dev_t dev,
    char* buffer,
    size_t buffer_length,
    const unsigned long timeout_milliseconds) {

  unsigned int size = 0;

  const millisecond_timestamp_t start_time = get_millisecond_timestamp();
  millisecond_timestamp_t elapsed = 0;

  while(1) {
    int bytes_read = voltronic_dev_read(
      dev,
      buffer,
      buffer_length,
      timeout_milliseconds - elapsed);

    if (bytes_read >= 0) {
      while(bytes_read) {
        --bytes_read;
        ++size;

        if (*buffer == END_OF_INPUT) {
          SET_ERRNO(0);
          return size;
        }

        buffer += sizeof(char);
        --buffer_length;
      }

      elapsed = get_millisecond_timestamp() - start_time;
      if (elapsed >= timeout_milliseconds) {
        SET_ERRNO(ETIMEDOUT);
        return -1;
      }

      if (buffer_length <= 0) {
        SET_ERRNO(ENOBUFS);
        return -1;
      }
    } else {
      return bytes_read;
    }
  }
}

static int voltronic_receive_data(
    const voltronic_dev_t dev,
    char* buffer,
    const size_t buffer_length,
    const unsigned long timeout_milliseconds) {

  const int result = voltronic_read_data_loop(
    dev,
    buffer,
    buffer_length,
    timeout_milliseconds);

  if (result >= 0) {
    if (((size_t) result) >= NON_DATA_SIZE) {
      const size_t data_size = result - NON_DATA_SIZE;
      const voltronic_crc_t read_crc = read_voltronic_crc(&buffer[data_size], NON_DATA_SIZE);
      const voltronic_crc_t calculated_crc = calculate_voltronic_crc(buffer, data_size);
      buffer[data_size] = 0;

      if (read_crc == calculated_crc) {
        SET_ERRNO(0);
        return data_size;
      }
    }

    SET_ERRNO(EBADMSG);
    return -1;
  } else {
    return result;
  }
}

static int voltronic_write_data_loop(
    const voltronic_dev_t dev,
    const char* buffer,
    size_t buffer_length,
    const unsigned long timeout_milliseconds) {

  const millisecond_timestamp_t start_time = get_millisecond_timestamp();
  millisecond_timestamp_t elapsed = 0;

  int bytes_left = buffer_length;
  while(1) {
    const int write_result = voltronic_dev_write(dev, buffer, bytes_left);

    if (write_result >= 0) {
      bytes_left -= write_result;
      if (bytes_left > 0) {
        buffer = &buffer[write_result];
      } else {
        return buffer_length;
      }

      elapsed = get_millisecond_timestamp() - start_time;
      if (elapsed >= timeout_milliseconds) {
        SET_ERRNO(ETIMEDOUT);
        return -1;
      }
    } else {
      return write_result;
    }
  }
}

static int voltronic_send_data(
    const voltronic_dev_t dev,
    const char* buffer,
    const size_t buffer_length,
    const unsigned long timeout_milliseconds) {

  const voltronic_crc_t crc = calculate_voltronic_crc(buffer, buffer_length);

  const size_t copy_length = buffer_length + NON_DATA_SIZE;
  char* copy = malloc(copy_length * sizeof(char));
  memcpy(copy, buffer, buffer_length * sizeof(char));

  write_voltronic_crc(crc, &copy[buffer_length], NON_DATA_SIZE);
  copy[copy_length - 1] = END_OF_INPUT;

  const int result = voltronic_write_data_loop(
    dev,
    copy,
    copy_length,
    timeout_milliseconds);

  free(copy);

  return result;
}

int voltronic_dev_execute(
    const voltronic_dev_t dev,
    const char* send_buffer,
    size_t send_buffer_length,
    char* receive_buffer,
    size_t receive_buffer_length,
    const unsigned long timeout_milliseconds) {

  const millisecond_timestamp_t start_time = get_millisecond_timestamp();
  millisecond_timestamp_t elapsed = 0;

  SET_ERRNO(0);
  const int send_result = voltronic_send_data(
    dev,
    send_buffer,
    send_buffer_length,
    timeout_milliseconds);

  if (send_result > 0) {
    elapsed = get_millisecond_timestamp() - start_time;
    if (elapsed < timeout_milliseconds) {
      SET_ERRNO(0);
      return voltronic_receive_data(
        dev,
        receive_buffer,
        receive_buffer_length,
        timeout_milliseconds - elapsed);
    } else {
      SET_ERRNO(ETIMEDOUT);
      return -1;
    }
  } else {
    return send_result;
  }
}
