# JT809DotNetty

由于脑子不够用，对于双链路的理解不是很到位，先解决目前需要对接的车辆数据。

## 作为上级平台（企业对企业）

目前只需要的是实时上传车辆定位信息。

> 注意：有些企业协议按照国标，但是链路没有遵循，所以企业对企业对接数据需要兼容不需要从链路的情况。

针对1对1的好处，可以根据企业的车辆数据进行负载的调整和业务的划分。

针对1对1的坏处，提高了维护成本。

### 数据接入流程

![superior_dataflow](https://github.com/SmallChi/JT809DotNetty/blob/master/doc/img/superior_dataflow.png)

### 数据处理

1. 从网关接收到GPS数据，解析GPS数据，再使用Google Protocol Buffer定义GPS数据结构存储在kafka中，这样可以跨语言开发。

2. 要是了解大数据存储及有能力运维的情况下，可以采用大数据替代方案。这里采用小众模式（主从模式），分表分区的方式存储，每天大概8千万左右。

### 使用例子

> 前提条件:需要安装kafka以及zookeeper

![superior_demo](https://github.com/SmallChi/JT809DotNetty/blob/master/doc/img/superior_demo.png)