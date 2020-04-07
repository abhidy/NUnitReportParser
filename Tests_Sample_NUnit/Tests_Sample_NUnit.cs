/**
\file

\project{CLS, Mondriaan}
\component{Test, Smoketest}

$LastChangedDate: 2014-06-24 11:09:31 +0200 (Tue, 24 Jun 2014) $
$Revision: 12715 $
$Author: erik.van.der.zanden@philips.com $

Copyright (c) 2008 Koninklijke Philips N.V.
All Rights Reserved.

This source code and any compilation or derivative thereof is the proprietary
information of Koninklijke Philips N.V. and is confidential in nature.
Under no circumstances is this software to be combined with any
Open Source Software in any way or placed under an Open Source License
of any type without the express written permission of Koninklijke Philips N.V.
*/

using System.IO;
using NUnit.Framework;
using System;

/// <summary>
/// Over The Air Upgrade tests
/// </summary>
namespace Tests_Sample_NUnit
{
    public class Tests_Sample_NUnit_Base_Class
    {
        [TearDown]
        public virtual void TearDown()
        {
            System.Console.WriteLine("TearDown virtual in class Tests_Sample_NUnit_Base_Class");
            //TearDownSpecific();
        }

        [SetUp]
        public virtual void SetUp()
        {
            System.Console.WriteLine("SetUp virtual in class Tests_Sample_NUnit_Base_Class");
            //SetUpSpecific();/
        }

        public virtual void TestFixtureTearDownSpecific()
        {
            System.Console.WriteLine("TestFixtureTearDownSpecific virtual in class Tests_Sample_NUnit_Base_Class");
        }
        public virtual void TestFixtureSetUpSpecific()
        {
            System.Console.WriteLine("TestFixtureSetUpSpecific virtual in class Tests_Sample_NUnit_Base_Class");
        }

        private void SetUpFlex()
        {
            try
            {
                //Flex.SetUp();
            }
            catch (Exception)
            {
                //Flex.TearDown();
                // Flex.SetUp();
            }
        }

        [TestFixtureTearDown]
        public virtual void TestFixtureTearDown()
        {
            System.Console.WriteLine("TestFixtureTearDown virtual in class Tests_Sample_NUnit_Base_Class");
            TestFixtureTearDownSpecific();
        }

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            System.Console.WriteLine("TestFixtureSetUp virtual in class Tests_Sample_NUnit_Base_Class");
            TestFixtureSetUpSpecific();
        }
    }

    [TestFixture]
    /// <summary>
    /// Upgrading image script
    /// </summary>
    public class Tests_Sample_NUnit_Fixture : Tests_Sample_NUnit_Base_Class
    {
        [TestCase(10,20,Result=30,TestName = "ResueatSector0",Category = "aaa")]
        [TestCase(10, 20, Result = 30, TestName = "ResueatSector1")]
        public int AAA_000_CreateZigBeeContainer(int a,int b)
        {
            return a + b;
        }
    }
}

