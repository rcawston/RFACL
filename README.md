# RFACL
XML configurable Windows ACL Utility

Usage: rfacl.exe [/q] [/qq] [/v] [/vv] {acl spec file} {path}

       /q   =  quiet mode (suppresses 'Press any key to exit' prompts)
       /qq  =  super quiet mode (no console output)
       /v   =  verbose mode
       /vv  =  super verbose mode (outputs XML configuration summary)

e.g.

    : rfacl.exe acl.xml c:\path
    : rfacl.exe c:\path\to\config\acl.xml c:\path
    : rfacl.exe /qq c:\path\to\config\acl.xml c:\path
    : rfacl.exe /v c:\path\to\config\acl.xml c:\path

# XML Config
See RFACL\ExampleConfig.xml for example
