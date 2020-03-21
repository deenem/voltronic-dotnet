#!/usr/bin/env python
# -*- coding: utf-8 -*-
#
#  rundisplay.py
#
#  Copyright 2020  <pi@rpi3e>
#
#  This program is free software; you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation; either version 2 of the License, or
#  (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program; if not, write to the Free Software
#  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
#  MA 02110-1301, USA.
#
#

import pika
import json
from collections import namedtuple
import statistics
import numpy


def drawHistory(draw):
    global history
    global current
    np = numpy.array(history)
    median = statistics.median(np[np != 0])
    if median > 0:
        for x in range(260):
            if current == (x+1)  or current == (x+2):
                lh = h / 3
                shape = [(x, h), (x, h - lh)]
                draw.line(shape, fill=1)
            if history[x] > 0:
                lh = ((history[x] * h) / (median)) / 3
                shape = [(x, h), (x, h - lh)]
                draw.line(shape, fill=0)


def showWatts(draw, watts):
    draw.text((10, 10), watts, font=font24, fill=0)


def showBatteryPct(draw, pct):
    draw.text((10, 40), pct, font=font24, fill=0)


def displayOperationalProps(payload):
    epd = epd2in7.EPD()
    epd.init()
    epd.Clear(0xFF)

    logging.info("epd2in7 Demo" + str(epd.height) + "x" + str(epd.width))
    Himage = Image.new('1', (epd.height, epd.width),
                       255)  # 255: clear the frame
    draw = ImageDraw.Draw(Himage)

    drawHistory(draw)
    showWatts(draw, "{0}W".format(payload.inverter.ACOutputActivePower))
    showBatteryPct(draw, "{0}%".format(payload.battery.BatteryCapacity))

    epd.display(epd.getbuffer(Himage))


def jsonToObject(message):
    payload = json.loads(message, object_hook=lambda d: namedtuple(
        'X', d.keys())(*d.values()))
    return payload


def callback(ch, method, properties, body):

    global count
    global current
    global history
    payload = jsonToObject(body)
    msgtype = properties.headers.get('type')

    if msgtype == 'OpProp':
        history[current] = payload.inverter.ACOutputActivePower
        current = current + 1
        current = current % w
        if count % 5 == 0:
            displayOperationalProps(payload)
            count = 0

    count += 1
    logging.info(" [x] Received {0} : {1}".format(
        payload, properties.headers.get('type')))


def queueread():

    credentials = pika.PlainCredentials('inverter', 'inverter')

    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='10.0.1.54', credentials=credentials))
    channel = connection.channel()

    channel.queue_declare(queue='inverter')

    channel.basic_consume(
        queue='inverter', on_message_callback=callback, auto_ack=True)

    print(' [*] Waiting for messages. To exit press CTRL+C')
    channel.start_consuming()


def main(args):
    return queueread()


if __name__ == '__main__':
    import sys
    import os
    import logging
    import time

    w = 264
    h = 176
    w2 = 132
    h2 = 88
    count = 0
    history = [0] * (w + 1)
    current = 1

    picdir = os.path.join(os.path.dirname(os.path.realpath(__file__)), 'pic')
    libdir = os.path.join(os.path.dirname(os.path.realpath(__file__)), 'lib')

    logging.basicConfig(level=logging.WARNING)

    if os.path.exists(libdir):
        sys.path.append(libdir)

    from waveshare_epd import epd2in7
    import time
    from PIL import Image, ImageDraw, ImageFont
    import traceback
    font24 = ImageFont.truetype(os.path.join(picdir, 'Font.ttc'), 32)
    font18 = ImageFont.truetype(os.path.join(picdir, 'Font.ttc'), 18)

    sys.exit(main(sys.argv))
