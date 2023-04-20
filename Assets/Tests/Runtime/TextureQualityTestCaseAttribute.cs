// Copyright 2020-2023 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using UnityEngine;

public class TextureQualityTestCaseAttribute : UnityEngine.TestTools.UnityTestAttribute, ITestBuilder {

    NUnitTestCaseBuilder m_Builder = new NUnitTestCaseBuilder();
    string m_SearchPattern;
    string m_SubFolder;

    public TextureQualityTestCaseAttribute(string searchPattern, string subFolder = null) {
        m_SearchPattern = searchPattern;
        m_SubFolder = subFolder;
    }

    IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite) {
        var results = new List<TestMethod>();

        try {
            var folder = m_SubFolder == null ? Application.streamingAssetsPath : $"{Application.streamingAssetsPath}/{m_SubFolder}";
            var directoryInfo = new DirectoryInfo(folder);
            var files = directoryInfo.GetFiles(m_SearchPattern);
            foreach (var fileInfo in files) {
                results.Add(CreateTestCase(method, suite, fileInfo));
            }
        }
        catch (Exception ex) {
            Console.WriteLine("Failed to generate testcases!");
            Debug.LogException(ex);
            throw;
        }

        Console.WriteLine("Generated {0} test cases.", results.Count);
        return results;
    }

    TestMethod CreateTestCase(IMethodInfo method, Test suite, FileSystemInfo fileInfo) {

        var ext = fileInfo.Extension;
        var name = fileInfo.Name;
        // name = name[..^ext.Length];
        name = name.Substring(0, name.Length-ext.Length);
        
        var originalPath = m_SubFolder == null ? fileInfo.Name : $"{m_SubFolder}/{fileInfo.Name}";
        var ktxEtc1sPath = m_SubFolder == null ? fileInfo.Name : $"{m_SubFolder}/{name}-etc1s.ktx2";
        var ktxUastcPath = m_SubFolder == null ? fileInfo.Name : $"{m_SubFolder}/{name}-zuastc.ktx2";
        
        var data = new TestCaseData(new object[] {
            originalPath,
            ktxEtc1sPath,
            ktxUastcPath,
        });
        data.SetName(name);
        data.ExpectedResult = new UnityEngine.Object();
        data.HasExpectedResult = true;

        var test = m_Builder.BuildTestMethod(method, suite, data);
        if (test.parms != null)
            test.parms.HasExpectedResult = false;

        test.Name = name;
        return test;
    }
}
