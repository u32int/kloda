# Project setup
First of all, if you are using microsoft vs-something or a jetbrains IDE, you are mostly on your own. Sorry.
My setup is commandline/linux based, but this might work or still be useful on other platforms hopefully :) Detailed steps follow.

#### Setup the dotnet CLI
On nixos the package I used was `dotnetCorePackages.sdk_9_0`

#### Download the EXILED dependency from nuget
`nuget` is a package manager for the dotnet/csharp ecosystem (kind of like `cargo` for rust or `pip` for python)
This should come builtin with the dotnet CLI I think.
Execute `dotnet add package exiled` while in the project directory.

##### Copy over/Link the server DLLs
You are gonna need to add references to some DLLs that come with the SCP:SL server, look at the list in `kloda.csproj`.
I am not sure if I am allowed to redistribute them and you are unlikely to be trying to build this without having access to server files anyways, so they are not included in this repo.
They should be located in `<server_path>/SCPSL_Data/Managed/***.dll`
Either change the links to point to your local SCP:SL server installation or copy them over to `./server_dlls`.

This should be it.
You can build the project with `dotnet build` and copy the output DLL to your Plugins folder etc.

# Ecosystem rant
The dotnet ecosystem is (from an outsider perspective) the single most complicated software/framework/language/library thing I have every encountered. Every single part has a different version (and since we are doing some assembly modifying plugin magic here, these need to be matched perfectly to whatever exiled uses) for instance the dotnet framework required by exiled is dotnet4.8, but (the default? the latest?) the coresponding csharp version is from like 17 billion years ago while all the official microsoft docs make use of new csharp features, so the csharp version was manually set in the project file to be 10.0 (pretty arbitrarly but works for what I wanted) which might at some point break something idk!!

On another note; if you are a SCP:SL plugin developer that has published a plugin intended to be opensource, _please_ consider adding an [appropriate license](https://opensource.org/licenses) to your project, thanks!
