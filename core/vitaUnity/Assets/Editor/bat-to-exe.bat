set START=start
set START_SINGLE=start-singlescreen
set START_SPLIT=start-splitscreen

set OPTIONS=-icon logoIcon.ico -x64 -overwrite -productname VITA

if exist %START%.exe del %START%.exe
if exist %START_SINGLE%.exe del %START_SINGLE%.exe
if exist %START_SPLIT%.exe del %START_SPLIT%.exe

Bat_To_Exe_Converter_x64.exe -bat %START%.bat        -save %START%.exe        %OPTIONS%
Bat_To_Exe_Converter_x64.exe -bat %START_SINGLE%.bat -save %START_SINGLE%.exe %OPTIONS%
Bat_To_Exe_Converter_x64.exe -bat %START_SPLIT%.bat  -save %START_SPLIT%.exe  %OPTIONS%


@rem sign the exe's
@rem if this fails, you need to add the certificate
@rem C:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\CertMgr.exe
@rem Import .pfx certificate.
@rem Cert is ICTcode_Windows.pfx, ask IT for password

call "%VS140COMNTOOLS%\..\..\VC\vcvarsall.bat" x86

signtool sign %START%.exe
signtool sign %START_SINGLE%.exe
signtool sign %START_SPLIT%.exe
