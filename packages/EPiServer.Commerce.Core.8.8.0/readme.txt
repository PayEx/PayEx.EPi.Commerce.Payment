EPiServer.Commerce.Core


INSTALLATION
============

Applying database transforms
----------------------------

Database transforms are never applied automatically when updating a package to avoid unintentional
changes, make sure you have a backup of the database before running the update command. The
Update-EPiDatabase will automatically detect all install packages that support the pattern for
transformation used by EPiServer.

- Open "Package Manager Console".
- Make sure "Default project" points to the web site.
- Execute Update-EPiDatabase in the console.

Important note: Update-EPiDatabase should only be run in context of front-end site. Do not run it for
Commerce Manager site.


IMPORTANT VERSION SPECIFIC NOTES
================================

New in 8.0.0
------------

From version 8.0, EPiServer Commerce has no dependencies on Common Framework. 
If your website also has no dependencies on Common Framework, it is safe to remove the Common Framework
components (assemblies, configuration and database objects) using the tools in
tools\remove_commonframework. Refer to the readme file in the same folder for more details.
Install package EPiServer.CommonFramework and EPiServer.Common.Gadgets if your site has features depend
on Common Framework.

For a full changelog, visit http://world.episerver.com/installupdates


TROUBLESHOOTING
===============

If you get issues running the site after upgrade, delete all files in the bin folder (always keep a backup
just in case) and rebuild the project to clean out old files that might be incompatible.

For more details, see the product specific information for Commerce on
http://world.episerver.com/installupdates