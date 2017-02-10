EPiServer.Logging.Log4Net
============

Starting from version 2.0.0, EPiServer.Logging.Log4Net uses a log4net version that is not compatible
with previous version (Nuget version 2.0.3 or assembly version 1.2.13). 

If your project or any referenced library is referencing a previous version of log4net, such a version 1.2.10, 
you may experience problems building your project after installing this package. If this is the case, make sure 
that the project in question is updated to either use the EPiServer.Logging API or the new log4net version.

Even if you are not referencing log4net you can still get an error if the old log4net version is still present
in your bin folder. If this is the case, simply delete the old log4net dll before recompiling.

If upgrading the log4net reference is not possible, please downgrade EPiServer.Logging.Log4Net to v1.x as this
version will maintain dependency to log4net 1.2.10.