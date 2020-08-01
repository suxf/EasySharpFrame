﻿using ES.Data.Database.SQLServer;
using ES.Data.Database.SQLServer.Linq;
using System;

namespace Sample
{
    /// <summary>
    /// 数据库测试类【Sqlserver专用】
    /// 此类包含数据库对象中所有内容
    /// 帮助使用框架的朋友进一步了解数据库创建和使用
    /// </summary>
    class Test_DBSqlServer
    {
        // 数据库助手对象
        SQLServerDBHelper dbHelper;
        public Test_DBSqlServer()
        {
            Console.WriteLine("数据测试开始");
            // 数据库连接使用此函数即可简单创建 数据库的创建还提供更多重载方案，可以点入查看
            dbHelper = new SQLServerDBHelper("127.0.0.1", "sa", "123456", "db_test");
            // 检测数据库连接是否成功调用 成功返回true
            if (dbHelper.CheckConnected())
            {
                Console.WriteLine("数据库已连接");
            }
            Console.WriteLine("数据库测试结束");
        }


        /// <summary>
        /// 数据库助手使用
        /// </summary>
        public void DBHelperUseDemo()
        {
            // 检测是否数据库连接正常
            dbHelper.CheckConnected();

            // 普通查询调用
            var result = dbHelper.CommandSQL("SELECT * FROM tb_test");
            // 查询条数判断
            if (result.effectNum > 0)
            {
                // 取出表一的相关数据
                // 如果查询有多个select 可以通过result.dataSet取得
                int id = (int)result.collection[0]["id"];
                Console.WriteLine($"id:{id}");
            }

            // 非查询sql调用
            var result2 = dbHelper.ExecuteSQL("UPDATE tb_test SET content = 'Hello' WHERE id = 1");
            // 返回影响记录数量
            if (result2 > 0)
            {
                Console.WriteLine("success");
            }

            // 使用sql构建器来执行sql
            // 这种方式快捷，但是也只能应付一些简单的数据处理
            var result5 = SQLBuilder.Create(dbHelper).Fields("id", "userid").Where("id > 0").Select();
            // 查询条数判断
            if (result5.effectNum > 0)
            {
                // 取出表一的相关数据
                // 如果查询有多个select 可以通过result.dataSet取得
                int id = (int)result5.collection[0]["id"];
                Console.WriteLine($"id:{id}");
            }

            // 存储过程调用
            // 此处创建参数可以有多中方式
            // 详情可以参考重载中的几种方式
            var result4 = dbHelper.Procedure("pr_test", Parameter.Create("@id", 1), "@id2".ToParameter(2));
            // 存储过程中返回已经默认写好了
            // 直接调用结果的变量即可得到，但需要根据返回进行强转
            if ((int)result4.returnValue == 0)
            {
                // 如果有select返回
                var count = result4.Tables.Count;
                Console.WriteLine($"count:{count}");
            }

            // 异步执行SQL
            // 某些数据更新不需要阻塞主线程且不需要即时返回数据
            // 可以通过此函数进行异步队列执行
            dbHelper.PushSQL("UPDATE tb_test SET content = 'Hello' WHERE id = 1");
            // 异步执行存储过程
            // 功能同 异步执行SQL
            dbHelper.PushProcedure("pr_test", Parameter.Create("@id", 1), "@id2".ToParameter(2));
        }

        /// <summary>
        /// 数据代理使用
        /// </summary>
        public void DataAgentUseDemo()
        {
            // 创建一个数据表代理
            // 代理是为了某些高频读写操作而设计的缓存
            // 代理可以事先根据条件读取一张表
            // 读取成功后可以长时间对表进行读和写
            var dbagentRows = dbHelper.LoadDataCache("id", "tb_test", "id > 0");
            // 读取表id为100的记录
            var row = dbagentRows[100];
            // 读取表id为100记录的content字段
            var content = row["content"];
            // 读取表id为100记录的content字段
            var content2 = row.GetObject<string>("content");
            Console.WriteLine($"content:{content},{content2}");
        }

        /// <summary>
        /// 非关系型数据助手使用
        /// </summary>
        public void NoDBStorageUseDemo()
        {
            // 框架引入一个菲关系型数据结构的概念设计
            // 通过新建一个NoDBStorage对象，确定键值的类型，这个类型和数据库中的字段类型对应即可，在写入一些必要条件，即可完成初始化
            // 这个类可以创建一个只存在键值关系的数据结构，创建之后即可通过对象进行快速的数据访问和存储
            // 注意一旦读取，数据将托管的在程序内存中，数据库只是用于持久化保存方案，所以如果键值关系是比较重要且处理频繁的数据
            // 切记不要直接修改数据库
            // 另外由于整个框架除了引入一些必要框架外 全部原生类和算法实现，所以对于数据的插入没有进行注入检测 需要根据各自需求进行检测
            var nodb = new NoDBStorage<int, string>(dbHelper, "id", "content", "tb_cus_accounts", 10000/* 这个时间为持久化更新周期 */);
            // 通过建立的对象来加入一个新的数据
            // 注意此对象所在的表 不能够存在 一些约束性规则 来阻止记录的插入 否则失败
            nodb.TryAdd(100, "hello world");

            Console.ReadLine();
            // 通过建立的对象来拾取数据
            // 如果存在则为true 并且返回数据对象
            if (nodb.TryGetValue(100, out string value))
            {
                Console.WriteLine($"value:{value}");
            }

            Console.ReadLine();
            // 通过建立的对象进行修改值
            // 修改的数据会周期性写入持久化数据库中
            nodb.SetValue(100, "hello world again!!!");
            // 立即刷新持久化数据
            // 当有需求需要立刻通过键值更新到持久化中的时候可以使用
            // 建议不要频繁使用，这样不就没价值了不是
            nodb.Flush();
            // 如果需要重新拉取数据，那么就调用这个吧
            // 清空函数会把内存中的数据清空，然后后续操作全部会重新读取数据库最新数据
            nodb.Clear();
        }
    }
}