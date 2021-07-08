To run service:

```
sudo cp startSM3MsgApp.service /etc/systemd/system/startSM3MsgApp.service     ~ copy file
sudo systemctl start startSM3MsgApp.service                                  ~ start service
sudo systemctl stop startSM3MsgApp.service  
```

To install service:
```
sudo systemctl enable startSM3MsgApp.service
```