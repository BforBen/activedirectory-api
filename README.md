Active Directory API
===================

This ASP.NET WebAPI project returns information about users. It is the latest version of the AD API and has not been extensively tested.

## Users

### v1/users/councillors?q=`{search term}`

Returns an [IUser](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) array of councillors, optionally filtered to the search term.

### v1/users/heads-of-service?q=`{search term}`

Returns an [IUser](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) array of heads of service, optionally filtered to the search term.

### v1/users?q=`{search term}`&rq=`{true|false}`

Returns an [IUser](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) array of users, optionally filtered to the search term. If `rq` is true, the query value will be returned as one of the entries in the array even if it is not found.

### v1/users/`{SamAccountName}`

Returns an [IUser](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) if found.

### v1/users/`{SamAccountName}`/groups?follow=`{true|false}`

Returns a string array of group memberships for the user. If follow is `true`, nested group membership will be returned.

### v1/users/`{SamAccountName}`/reports

Returns an [IUser](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) array of direct reports for the user, if found.

## Groups

### v1/groups?q=`{search term}`

Returns an array of [IGroup](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) that match the search term.

### v1/groups/`{GroupSamName}`

Returns an [IGroup](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) if found.

### v1/groups/`{GroupSamName}`/members

Returns an array of [IUser](https://github.com/GuildfordBC/activedirectory/blob/master/Interfaces/ActiveDirectory.cs) of members of the group.

## Services

### v1/services?IncludeXMT=`{true|false}`

Returns a string array service units optionally including Corporate Management Team.
