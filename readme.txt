In production:

1) Select some port 
   (forexample: 8050)
2) Set  that selected port for pipe fields selfPort and remotePort
   (forexample: selfPort = 8050; remotePort = 8050;)
3) Build both project instance with the same setting.


In debug on the one device:

1) Select couple of ports
   (forexample: 8051 and 8052)
2) For "instance A" and "instance B" set pipe fields selfPort and remotePort in cross-over manner
   (forexample:
    for "A": selfPort = 8051; remotePort = 8052;
	for "B": selfPort = 8052; remotePort = 8051;)
