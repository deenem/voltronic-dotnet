# Installing as a SystemD service on linux

Once you have the inverter.service project runnign from the command line you can install it as a system service

The is an example unit file in the setup folder. You shoud just have to change the WorkingDirectory and ExecStart settings

Do the following to install the service
sudo cp ./setup/inverter.service /etc/systemd/system/
sudo chmod +x /etc/systemd/system/inverter.service
sudo systemctl daemon-reload
sudo systemctl status inverter

If all went well then you can try and start the service with

sudo systemctl start inverter.service

You can chek the /var/log/system for if there any any errors