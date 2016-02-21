# IBMarketRecorder
Interactive Brokers Market Data Recorder

This project and this file are licensed under the terms of the MIT license. THIS SOFTWARE COMES WITH NO WARRANTIES, USE AT YOUR OWN RISK.

This is a tool that captures live quotes for a set of symbols and saves this raw data in 1-second snapshots on a MS SQL database. It works with the TWS Interactive brokers(IB) application. 

The information stored is equivalent to recording live streaming quotes directly to do a database. Each tick time, data-type, symbol and value are captures so the stream could be reproduced if needed.

There is a second program called BriefMaker that takes these 1-second snapshots and converts them into 6-second summaries (I call the briefs). A brief would contain the high/low/last/vol/etc for every symbol in that 6 second interval. Briefs are a more a more intelegent "ordered" version of the steamMoment however there is some data loss. The time and value of each individual tick cannot be reproduced for example.

Each tick is saved in the following format: (7 bytes total)
 - current millisecond (stored as a byte from 0 to 99)
 - ticker Type - holds the tick data type high/low/last/vol/etc. (stored as byte)
 - ticker ID - this is the symbol ID like AMD, INTC, ^DJI (stored as byte)
 - Value - this is the actual data like volume, price, etc. (stored as float)
 
After each second completes a new row is added to the StreamMoments table. Each row has a SnapshotTime(DateTime) and Data(image) column.
 
The symbols that it downloads are located in the Symbols Table.  It is preloaded with examples. 
 
##Features
 - NLog logging
 - Very fast - the program is designed around low latency. I came up with a "coin" class for this. One side of the coin is capturing while the other is uploaded to the DB.
 - Direct network connection to other applications for low latency connections - each 1-second snapshot can be transported directly to another application bypassing the database. The database record is still written as well.  The main reason I added this was to reduce latency to another application I built called "BriefMaker".
 - Built-in auto-reconnect features in case the connection is lost.

##How to use
1) Mount the included database (or create your own with the same specs). This has two SQL tables. One is the list of symbols and the second are the snapshots. I would use the name "focus" for the database name.
2) Update the .config file:
    - If the database name is not "focus" then you will need to update this in the connection string in the .config file.
	- Below you will see IBHostToConnectTo and IBPortToConnectTo... adjust if needed.
	- Set the ShutDownTime as need.  The program will automatically close as needed.  I am in the PST time zone so that is why it is defaulted to 14:15(2:15PM). 
3)  In the Interactive Brokers TWS and go to File -> Global Configuration -> API -> Settings.  Check the box "Enable ActiveX and socket clients".  You might also need to add 127.0.0.1 in the Trusted IP Addresses. Also note the port number.  This port number should be the same as the one in the .confg file.
4) Start the MarketRecorder and it should connect. (view the logs for details on why it might not connect)

##Other functions
 - "Delete All" Button - This will erase all the contents in the StreamMoments table.