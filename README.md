# README #

Zylab.Interview.BinStorage 


### Solution descriptions ###

* I chose b+tree with log(n) complexity for index 
* Index cache size limited by 200Mb, const can be changed 
* Support for concurrency, with minimal locking
* Performance optimization for parallel writing + md5 calculation
* RAM writing and reading buffers are limited in const ~81Kb (less than .NET LOH object size)
* Constant memory consumption per thread

### Used open source libraries ###

* BPlusTree (https://github.com/csharptest/CSharpTest.Net.Collections)
* NuGet (https://www.nuget.org)

### Solution limitation ###

* I have not provide thread pool or any limits on thread number
* I wouldn't implement custom btree, lru cache, sorry