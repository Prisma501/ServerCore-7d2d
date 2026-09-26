---
title: ClaimCreator
description: Use the ClaimCreator web UI to draw advanced claims and reset regions directly on the map.
---

## Intro

ServerCore has many advanced claim types and reset regions funcionality. All those areas can be created with consolecommands. For ease of use ServerCore has a built in claim creator.
With this Web UI its very easy to create advanced claims or mark regions for reset. Just draw the area and click your way through creating claims.

To access the Claim Creator Web UI just add one to the port you are using to access allocs webmap.
So if you are using `http://serverip:8082` to access allocs webmap, you add 1 to that port and thus access ClaimCreator with `http://serverip:8083`

![Allocs webmap view of the ClaimCreator web UI showing the in-game world map with roads, towns and biomes](../../../assets/claimcreator/complete1.webp)

## Using ClaimCreator

For using the webui you must login via Steam so ServerCore can determine what permissions you have on the map. Default permission level for all is admin level 0.

![ClaimCreator login menu showing "Not logged in" with a "Log in via Steam" option](../../../assets/claimcreator/login1.webp)

If you want to change the default permissions for ClaimCreator create claims, view online players, view landclaims, view advClaims and (quest) POI's, you can do so in ClaimCreator_permissions.xml. That file is in your Saves folder.

````
<?xml version="1.0" encoding="UTF-8"?>
<PrismaCore_permissions>
	<permissions>
		<permission module="PrismaCore.map" permission_level="0" />
		<permission module="PrismaCore.createadvclaims" permission_level="0" />
		<permission module="PrismaCore.getlandclaims" permission_level="0" />
		<permission module="PrismaCore.getadvclaims" permission_level="0" />
		<permission module="PrismaCore.getresetregions" permission_level="0" />
		<permission module="PrismaCore.getplayerhomes" permission_level="0" />
		<permission module="PrismaCore.getplayersonline" permission_level="0" />
		<permission module="PrismaCore.getquestpois" permission_level="0" />
		<permission module="PrismaCore.getallpois" permission_level="0" />
		<permission module="PrismaCore.gettraders" permission_level="0" />
		<permission module="PrismaCore.getvehicles" permission_level="0" />
	</permissions>
</PrismaCore_permissions>
````

### Creating advanced claims

Make sure you have selected "Select area" in the bottom right selection area.

![Selection mode buttons in the bottom right of the map: Select area, Select region, and Clear selection](../../../assets/claimcreator/navarea.webp)

Now it's as easy as selecting 2 opposing corners of an area you want to mark. The area will automatically be drawn on the map for you to review.

In the following example i will make a hostile free advanced claim on an entire city. The steps are always the same for creating all types of advanced claims:

Draw the desired area:

![A rectangular area drawn on the map, highlighted in blue, covering a town](../../../assets/claimcreator/selection.webp)

Now select "Claims" in navigation menu.

![The "Claims" entry in the ClaimCreator navigation menu](../../../assets/claimcreator/claims.webp)

This window will show all your defined advanced claims and reset regions and can be managed from here.

![Claims dialog listing existing claims with Create and Delete claim(s) buttons](../../../assets/claimcreator/claims_overview.webp)

We are going to add the selected area on map as a new claim. Click "Create".

![Create a claim dialog showing the generated console command, name, access level and claim type fields](../../../assets/claimcreator/createclaim.webp)

Now all you have to do is give your claim a unique name and assign an accesslevel. Then select the type of advanced claim in the dropdown menu. Each type of claim will show you a brief explenation and offers you the possibility to configure all claim parameters (if present).

For my hostilefree advanced claim it looks like this:

![Create a claim dialog with the hostilefree claim type selected, showing its description and generated command](../../../assets/claimcreator/hostilefree.webp)

You can repeat above steps to create more and different types of advanced claims.

The "Commands" button in navigation menu will show you the number of commands that are awaiting execution (claim creation). In my example that number is 1 as i created only 1 advanced claim.

![The "Commands" entry in the navigation menu showing a badge with 1 pending command](../../../assets/claimcreator/commands.webp)

Click "Commands"

An overwiew of commands to be executed is presented:

![Commands dialog listing the pending ccc add command with Execute and Delete buttons, plus a History section](../../../assets/claimcreator/commandsoverview.webp)

You can review your commands and if all is well, click the green "Execute x commands" button. All claims in the list will be created and are immediately visible on ClaimCreator and Allocs Webmap.

### Creating Reset Regions

Make sure you have selected "Select region" in the bottom right selection area.

![Selection mode buttons with "Select region" highlighted](../../../assets/claimcreator/navregion.webp)

Now you can select regions (make the grid visible via floating menu -> Regions) by just clicking on them.

![Map view with the region grid overlay visible, showing region coordinates such as r.-3.6.7rg](../../../assets/claimcreator/regions.webp)

You can select as many as you like and when you are done just do the same steps as above when creating an advanced claim.

Those steps are:

Click "Claims" in navigation menu -> Click "Create" -> Click "Commands" in navigation menu -> Click "Execute x commands"

All your reset regions have been created and are active immediately including enter/exit notification and automatic landclaim removal.

## Overview of all advanced claims and parameters

### hostilefree:

![Create a claim dialog with the hostilefree claim type selected, showing its description and generated command](../../../assets/claimcreator/hf.webp)

### notify:

![Create a claim dialog with the notify claim type selected, showing entering and exiting text fields](../../../assets/claimcreator/notify.webp)

### command:

![Create a claim dialog with the command claim type selected, showing the command to execute field](../../../assets/claimcreator/command.webp)

### leveled:

![Create a claim dialog with the leveled claim type selected, showing Y coordinate top and bottom fields](../../../assets/claimcreator/leveled.webp)

### reversed:

![Create a claim dialog with the reversed claim type selected and its description shown](../../../assets/claimcreator/reversed.webp)

### normal:

![Create a claim dialog with the normal claim type selected](../../../assets/claimcreator/normal.webp)

### timed:

![Create a claim dialog with the timed claim type selected, showing the hours before claim vanishes field](../../../assets/claimcreator/timed.webp)

### portal:

![Create a claim dialog with the portal claim type selected, showing step height and teleport destination coordinate fields](../../../assets/claimcreator/portal.webp)

### openhours:

![Create a claim dialog with the openhours claim type selected, showing opening and closing hour fields](../../../assets/claimcreator/openhours.webp)

### playerlevel:

![Create a claim dialog with the playerlevel claim type selected, showing the operator check field](../../../assets/claimcreator/playerlevel.webp)

### lcbfree:

![Create a claim dialog with the lcbfree claim type selected and its description shown](../../../assets/claimcreator/lcbfree.webp)

### antiblock:

![Create a claim dialog with the antiblock claim type selected, showing the forbidden blocks field](../../../assets/claimcreator/antiblocks.webp)

### reset:

![Create a claim dialog with the reset claim type selected](../../../assets/claimcreator/reset.webp)
