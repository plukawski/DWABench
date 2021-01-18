# DWABench - .NET Web API benchmarking tool.

**DWABench** is a real-world Windows based benchmarking tool that evaluates computer's hardware capabilities in regards of WebApi like workloads and SQL database like workloads. It tests not only your CPU, but also memory and disk using workloads that are similiar to the ones found in real web applications based on a `.NET 5` platform.

## Technical Description
The main goal of this tool is to be as simple as possible - to have a possibility to benchmark any Windows based system for webapi application related workloads without the need of any manual configuration (like setting up WWW server, SQL databases, etc.). It accomplishes that by starting its own www server (the `Kestrel` one) and using `Sqlite` as an SQL database engine which is ACID compliant. As a result the tool is not doing any changes in the system and also does not require any preconfiguration nor admin rights to make it work. Just download the ZIP package, extract it, run a single EXE file and get the results.

During benchmarking a real HTTPS secure connection on the loopback address is established between the client and the server.

Like the [Cinebench](https://www.maxon.net/en/cinebench) benchmark, which is a benchmark for 3d rendering applications, the goal of this tool was to create a benchmark utilizing webapi application like workloads having similar complexity for the end users (absolutely no manual preconfiguration needed). Cinebench is great to know the maximum theoretical performance only of the processor in the system, however that may not give a clear picture how a particular hardware will behave for workloads used in webapi like applications.

## Requirements
Any device or virtual machine with **Windows 7/Windows Server 2008 R2** or newer. It may work on older Windows versions but was not tested.

## How to use
Simply extract the ZIP package of the most recent release to a separate folder and run the `DWABench.exe` file. After the benchmark finishes it will create a CSV file with the results for the current day in the `Results` folder. When running the benchmark multiple times on the same day it will append the results to the CSV file automatically.

Here are examples of possible executions with different parameters:

- `DWABench.exe /memory=true` - will force the benchmark to use SQL database located in RAM instead of a disk
- `DWABench.exe /phase1-records=20000` - will alter the phase 1 of the benchmark to import 20000 records instead of the default 10000.
- `DWABench.exe /phase2-seconds=60` - will alter the phase 2 of the benchmark to execute for 60 seconds instead of the default 40.
- `DWABench.exe /memory=true /phase1-records=20000 /phase2-seconds=60` - will force the benchmark to use database located in RAM instead of a disk and will alter the phase 1 of the benchmark to import 20000 records instead of the default 10000 and will alter the phase 2 of the benchmark to execute for 60 seconds instead of the default 40.

When no parameters are specified the default values for them are used, that is benchmark will store database on a disk, will import 10000 records and will wait 40 seconds in phase 2.

## Main technology stack used in DWABench
- [Microsoft .NET 5](https://docs.microsoft.com/pl-pl/dotnet/core/dotnet-five)
- [Kestrel](https://docs.microsoft.com/pl-pl/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-5.0) WWW server (spawned in a separate process to the benchmark itself)
- [Sqlite 3](https://www.sqlite.org/) (Northwind database schema with some sample data taken from [here](https://github.com/jpwhite3/northwind-SQLite3))
- [IdentityServer4](https://github.com/IdentityServer/IdentityServer4)
- [Refit](https://github.com/reactiveui/refit)

## Benchmark phases description
- **Phase 1** - Executes an import of many product records into the database by executing `POST /products` endpoint in the loop. By default it imports 10000 products (configurable using command line parameter). The results of that phase can be used to directly compare the disk and IO subsystem performance for **SQL database related workloads** which forces the unbuffered writes to the disk to achieve durability.
- **Phase 2** - Executes typical user scenario for a web shop (3 product searches, adding a new order and getting list of orders) in a loop using number of threads which is equal to number of logical cores minus one in a given time period (defaults to 40 seconds). The results of that phase can be used to directly compare CPU and RAM performance for **webapi server and client related workloads**.

**NOTE:** The benchmark can be run using in memory database by setting parameter `memory` to true (`/memory=true`). In such case the disk will not be touched by the benchmark and it will test only CPU and RAM performance leaving the disk performance unmeasured. This is usefull if you don't want the physical disk performance to affect the results of other resources.

## Hardware characteristics important for the webapi applications
- **Unbuffered disk performance** - every SQL database by default makes unbuffered writes to the disk for durability, which means that the **WRITE CACHE** is not used. You can find that many theoretically "fast" SSDs can be slower or only slightly faster than old mechanical drives when **WRITE CACHE** is disabled. See the following [article](https://www.percona.com/blog/2018/07/18/why-consumer-ssd-reviews-are-useless-for-database-performance-use-case/) for more details.
- **Low memory latency and fast single core speed** - each http request should be handled using the same speed and as fast as possible, so access to a different parts of memory should be fast and speed of the single CPU core should also be fast to decrease the time needed to handle a single request
- **Ability to handle many parallel requests (big multicore performance)** - in real-world, when webapi application is deployed on production there are very few situations when we deal with single requests. Much often the webapi application must handle a lot (hundreds or thousands) of multiple requests per second. To be able to handle such big amount of parallel requests from multiple users we need a system with many cores and good caching performance to be able to process them all in a reasonable time.

Because this benchmark uses logic which is very similar to the real-world webapi application (real web server, real ACID compliant SQL database, real HTTPS connection with OAuth 2 authentication, etc.) it should be well suited to measure the above aspects of the system hardware.

## License
Copyright 2020-2021 Przemysław Łukawski

Distributed under the MIT license