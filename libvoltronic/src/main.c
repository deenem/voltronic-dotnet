#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "voltronic_dev_usb.h"

int main (int argc, char *argv[])
{

  const char* serial_number = 0; // Optional
  voltronic_dev_t dev = voltronic_usb_create(0x0665, 0x5161, serial_number);

  if (dev == 0) {
    printf("Could not open USB device with serial number %s\n", serial_number);
    return 1;
  }

  const char* command = "QPIGS";
  if (argc > 1){
      command = argv[1];
  }
  
  // Query the device
  char buffer[128];
  int result = voltronic_dev_execute(dev, command, strlen(command), buffer, sizeof(buffer), 1000);
  if (result > 0) {
    printf("SUCCESS:%s=>%s\n", command, buffer);
  } else {
    printf("Failed to execute %s\n", command);
  }

  // Close the connection to the device
  voltronic_dev_close(dev);
}

