
Param ( 
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()] 
        [string] 
        $EndPoint
    , 
        [Parameter(Mandatory=$true, Position=1)]
        [int]
        $Port
    , 
        [Parameter(Mandatory=$true, Position=2)]
        [string]
        $Message
) 
Process {
    # Setup connection 
    $IP = [System.Net.Dns]::GetHostAddresses($EndPoint) 
    $Address = [System.Net.IPAddress]::Parse($IP) 
    $Socket = New-Object System.Net.Sockets.TCPClient($Address,$Port) 
    
    # Setup stream wrtier 
    $Stream = $Socket.GetStream() 
    $Writer = New-Object System.IO.StreamWriter($Stream)

    # Write message to stream
    $Message | % {
        $Writer.Write($_)
        $Writer.Flush()
    }

    Start-Sleep -s 2
    
    # Close connection and stream
    $Stream.Close()
    $Socket.Close()
    }
