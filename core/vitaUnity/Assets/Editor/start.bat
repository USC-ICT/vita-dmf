pushd %~dp0\data
start vita.exe -vitaserver -logfile vita_Data\output_log_server.txt -screen-width 800 -screen-height 600 -screen-fullscreen 0
@rem ping 127.0.0.1 -n 6 > nul
start vita.exe -vitaclient -logfile vita_Data\output_log_client.txt -screen-width 800 -screen-height 600 -screen-fullscreen 0
popd
