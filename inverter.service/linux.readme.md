# Installing as a SystemD service on linux

Once you have the inverter.service project running from the command line you can install it as a system service

The is an example unit file in the setup folder. You shoud just have to change the WorkingDirectory and ExecStart settings

Do the following to install the service
sudo cp ./systemd.unit/inverter.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl status inverter.service

If all went well then you can try and start the service with

sudo systemctl start inverter.service

You can check the /var/log/syslog for if there any any errors
Usually the error is that dotnet is not installed to /usr/share/dotnet 
Since running as a service doesn't use your pi user environment dotnet needs ot be installed globally

You also need to create a file /etc/profile.d/dotnet.sh that contains the following lines

export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:/usr/share/dotnet 

If everything is working you can enable the service with 

sudo systemctl enable inverter.service

and it should now start up automatically