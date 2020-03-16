# voltronic-dotnet
.Net Core 3.1 based system to control the Voltronic class of inverters

## Objective 
The Voltronic group of inverters (MKS, VM and King) are re-branded under many names, Axpert. MPP, Infinisolar, Mecer, Kodak and RCT to mention a few. 
After installing an RCT VM III 5K inverter and Pylontech US3000 battery in my home I was interested in developing software to interface with the USB port on the inverter. 
I found many examples in C++ and Python but my environment of choice is .Net and so I set out to create a system written in .Net core 3.1 to allow it to be as portable as possible. 
The goal is to be modular, there will be many small, loosely coupled system components, rather than one big system. This will allow it to be extensible and easy to modify.

## Architecture
The core component will be a linux service that will manage all communications with the inverter via the USB port. 
The port does not seem to work well with multiple clients accessing it at the same time, so I plan to have one service running which will handle all comms. The service will query the inverter periodically and then publish the results to a Rabbit MQ message queue. All other system compononents will query the Rabbit MQ queue for data. The service will also listen on a queue for requests to send new Settings commands to the inverter. The goal is to create a logical data model to represent the inverter status and then allow clients to modify the data model and place it onto the queue. The service will translate the changes to the data model into settings commands to send to the inverter. The entire process must be asynchonous, changes to the data model will reflect in new status messages on the queue when they have been affected.

I have not found any .net libraries that will allow me to communicate with my inverter over the /dev/hidraw0 device yet, so I have used the libvoltronic (https://github.com/jvandervyver/libvoltronic) c++ library to do the communications for me.
I have just modified the library slightly to pass commands to the inverter on the command line.

## The System Components

0. inverter.common, This is a class library with all the shared code that all components will use
1. inverter.comms , The core component, it's role is to read and write to theinverter via the USB port. It uses Rabbit MQ to facilitate communicatiosn with all the other components
2. inverter.log, Used for debugging or log gathering. It monitors the Rabbit MQ queues and logs all messages either to the console or to a log file
3. inverter.db, This component monitors the queue for any update messages and write the updates to a database table or NoSQL data store. Can be configured to write to multiple destinations
4. inverter.api, This component exposes a WebSocket API for any clients to connect to and receive status updates from the inverter. The component monitors the queue and pushes any updates received onto the WebSokcet clients.
                  I also exposes a JSON-RPC interface which can be used to change inveter settings.
5. inverter.web, This is a website written in (Vue/React?) which can be used to view the inverter status and send instrctions to the inverter.
6. inverter.mqtt , This component will post mqtt messages to an mqtt server. Inspired by the Voltronic HomeAssisant Project , https://github.com/ned-kelly/docker-voltronic-homeassistant
7. inverter.cloud, Will post data to cloud based information aggregators such as EmonCMS 

## Setup

## Hardware
You will need a machine which you can plug into the USB port on the inverter. The obvious choice is a Raspberry Pi, but the only real requirement is somethig which can run .Net Core 3.1. 

## Software


## Rabbit MQ
You need to install Rabbit MQ Server somewhere on your server when all components can reach it.
Clients not access queues from 'localhost' ned to be authenticated so I have used inverter/inverter as the username/password combination throughout . You might want to change this if you plan on exposing the queue on the internet.
Generally you can change these settings in the appsetting.json file under:

  "ConnectionStrings": {
    "RabbitMQHost": "10.0.1.54",
    "RabbitMQUser": "inverter",
    "RabbitMQPass": "inverter"
  }

After initial setup you need to create the user ans set premissions as follows

$ sudo rabbitmqctl add_user inverter inverter
$ sudo rabbitmqctl set_permissions inverter ".*" ".*" ".*"

# libvoltronic
Before you can run the inverter.comms service you must have a working 

The software has been designed so that different components can run on different machines. 
You can run all the software components on the same machine, but the minimum required to do this would be a Raspberry Pi 4 (4Gb)

At a minimum you need to run inverter.comms on the machine plugged into the inverter. If you chose to install Rabbit MQ on a different machine you set the Rabbit MQ server address in the appsettings.json for inverter.comms

All the other components can run on any other machine on the same network. The only requirement is that each component must be able to connect to the Rabbit MQ server. The server address needs to be set in appSettings.json for each component.
