sdetools
========

This is a collection of tools for assisting with automating deployments and working with ESRI .sde files.

### Projects

 - Core: Class library with shared content
 - gdbconfig: Windows Console Application for perform configuration tasks against an ESRI geodatabase
 - sde2string: Windows Console Application for decoding and testing a .sde file

---

## gdbconfig

This Windows console application will perform various configuration tasks against a target ESRI geodatabase, and can help with automated deployments. The full options are detailed below and are also available with the ``--help`` switch.

### Usage and Options

```
 > gdbconfig.exe -h
 > gdbconfig --test -i=connection.sde {Options}
 > gdbconfig gdb.sde --add-domain    <Domain> <Value> [<Name>]
 > gdbconfig gdb.sde --list-domain   <Domain>
 > gdbconfig gdb.sde --order-domain  <Domain> [VALUE|Name] [ASC|DESC]
 > gdbconfig gdb.sde --remove-domain <Domain> <Value>
 > gdbconfig gdb.sde --add-class-model-name    <ClassName> <ModelName>
 > gdbconfig gdb.sde --list-class-model-name   <ClassName>
 > gdbconfig gdb.sde --remove-class-model-name <ClassName> <ModelName>
 > gdbconfig gdb.sde --add-field-model-name    <ClassName> <FieldName> <ModelName>
 > gdbconfig gdb.sde --list-field-model-name   <ClassName> <FieldName>
 > gdbconfig gdb.sde --remove-field-model-name <ClassName> <FieldName> <ModelName>
 > gdbconfig --dry-run -p1 <Domain> -p2 <Value> -p3 <Name> --input connection.sde -Vp
 > gdbconfig [? | /? | -? | -h | --help | --version]

Options:
 -i/--input     : Path to an .sde file for connection to an ESRI geodatabse
 -t/--test      : Test the connnection and exit
 -d/--dry-run   : Perform a dry run without making changes
 -v/--version   : Report the version and exit
 -V/--verbose   : Add additional output
 -n/--nonewline : Output without trailing newline
 -l/--list      : List result after command execution
 -p/--pause     : Pause before exiting
```

### Release History:

  - v2.0 2017.03.15 Initial Release

---

## sde2string

This Windows console application will return the Connection Properties of a passed in .sde file. By default this will be in 'Connection String' syntax; ``[Key1]=Value;[Key2]=Value;``. The ``-l`` / ``--list`` switch will list out the properties on separate lines for easier reading, and ``-b`` / ``--bracketless`` will drop the brackets; ``Key1=Value;Key2=Value;``. The full options are detailed below and are also available with the ``--help`` switch (or parameterless invokation).

### Usage and Options

```
 > sde2string.exe -h
 > sde2string -input=connection.sde {Options}
 > sde2string gdb.sde --connect --list
 > sde2string gdb.sde -cl
 > sde2string gdb.sde -clVbpn
 > sde2string -v
 > sde2string --list --connect --input connection.sde -Vp
 > sde2string [? | /? | -? | -h | --help | --version]

Options:

 -i/--input       : Input .sde file to be processed
 -c/--connect     : Attempts to establish a connection and examines result
 -V/--verbose     : Show additional output
 -r/--raw         : Output the raw (semi-parsed) contents of the .sde file as ascii
 -l/--list        : Lists each property on a single line
 -u/--unparsed    : Output the raw (unparsed) contents of the .sde file as ascii
 -b/--bracketless : Display key values without surrounding brackets
 -e/--encoding    : Specify the .sde encoding. May cause errors without -u (unparsed)
 -n/--nonewline   : Output without trailing newline
 -p/--pause       : Pause for a key press before terminating
 -v/--version     : Display Version and Exit
 -h/--help        : Display this help (also ?, -?, /?, or no input)

Encodings: DEFAULT, ASCII, UTF7, UTF8, UTF16, UTF32
```

### Release History
  - v2.0 2017.03.15 Restructured Core components into a separate project
  - v1.3 2014.07.17 Adds direct parsing, establishing a connection not the default
  - v1.2 2014.07.16 Adds the --pause option to leave the console window open
  - v1.1 2014.07.16 This embeds the necessary resources for a standalone .exe and adds the --bracketless option
  - v1.0 2014.07.15 This release is a functional parser to read the connection properties from a .sde file

---

### Acknowledgements:

Thanks to Michael Juvrud for providing sample .NET ArcObjects code snippets and Jason Roebuck for sample .sde files.

---

### Contributors:

- Eric Menze ([@Ehryk42](https://twitter.com/Ehryk42))
- Michael Juvrud ([@mikejuvrud](https://twitter.com/mikejuvrud))

---

### Resources:

  - [ArcObjects SDK .NET Reference - 10.0](http://help.arcgis.com/en/sdk/10.0/arcobjects_net/componenthelp/index.html#/Overview/001m00000039000000/)
  - [Creating .sde files in ArcCatalog](http://resources.arcgis.com/en/help/main/10.1/index.html#//0017000000pt000000)

---

### Build Requirements:

  - Visual Studio (Built with Visual Studio 2017)
  - nuget (Visual Studio Package Manager)
    - Packages:
    - [log4net](https://www.nuget.org/packages/log4net/) - Configurable Logging
    - [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/) - Command Line Parsing
    - [Fody](https://www.nuget.org/packages/Fody/) - .NET Assembly Weaving
    - [Costura.Fody](https://www.nuget.org/packages/Costura.Fody/) - Making the .exe a standalone executable

---

### Contacts:

 - **Eric Menze**
   - [Email](mailto:rhaistlin+gh@gmail.com)
   - [Portfolio](http://ericmenze.com)
   - [Github](https://github.com/Ehryk)
   - [Twitter](https://twitter.com/Ehryk42)
   - [Source Code](https://github.com/Ehryk/sde2string)
