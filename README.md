<!--
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
-->

# microsoft-avro

This repository contains the Avro core extracted from the HDInsight SDK and
ported to the .NET Standard API on the .NET Core 2.0 runtime. It was last
tested against the 2.0.0-preview2-005858 version of the .NET Core tools and
runtime.

Prerequisites
-------------

  * Windows OS including Windows 10 or Windows Server 2016.
  * .NET Cli tools: [https://github.com/dotnet/cli](https://github.com/dotnet/cli)


Instructions
------------

You will need to build at the command line using the .NET Core SDK tools (dotnet)
since building in Visual Studio 2017 is not yet supported. However, the solution
can be opened in Visual Studio 2017. .NET Core applications will run on both
Windows and Linux.

    dotnet clean MicrosoftAvro.sln
    dotnet restore MicrosoftAvro.sln
    dotnet build -c Relase/Debug MicrosoftAvro.sln

 * `clean` cleans any previously built binary
 * `restore` restores nuget dependencies
 * `build` builds the solution

To build nuget packages, type the following:

 * `dotnet pack MicrosoftAvro.sln`

Projects will build out to `Microsoft-Avro\bin\<AssemblyName>` and will contain
all the platforms that the project supports.
