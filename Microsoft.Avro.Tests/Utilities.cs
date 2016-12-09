// Copyright (c) Microsoft Corporation
// All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

namespace Microsoft.Hadoop.Avro.Tests
{
    using Microsoft.Hadoop.Avro.Schema;
    using Microsoft.Hadoop.Avro.Utils;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using Xunit;

    public static class Utilities
    {
        [DataContract]
        public enum RandomEnumeration
        {
            Value0,
            Value1,
            Value2,
            Value3,
            Value4,
            Value5,
            Value6,
            Value7,
            Value8,
            Value9
        }

        private static readonly Random Random = new Random(13);

        public static Dictionary<Type, object> RandomGenerator = new Dictionary<Type, object>
        {
            { typeof(int), new Func<int>(Random.Next) },
            { typeof(double), new Func<double>(Random.NextDouble) },
            { typeof(string),  new Func<string>(() => "string" + Random.Next()) },
            { typeof(float), new Func<float>(() => (float)Random.NextDouble()) },
            { typeof(bool), new Func<bool>(() => Random.Next() % 2 == 0) },
            { typeof(long), new Func<long>(() => (long)Random.Next()) },
            { typeof(char), new Func<char>(() => (char)Random.Next('a', 'z')) },
            { typeof(byte), new Func<byte>(() => unchecked((byte)Random.Next())) },
            { typeof(sbyte), new Func<sbyte>(() => unchecked((sbyte)Random.Next())) },
            { typeof(short), new Func<short>(() => unchecked((short)Random.Next())) },
            { typeof(ushort), new Func<ushort>(() => unchecked((ushort)Random.Next())) },
            { typeof(uint), new Func<uint>(() => unchecked((uint)Random.Next())) },
            { typeof(ulong), new Func<ulong>(() => unchecked((ulong)Random.Next())) },
            { typeof(decimal), new Func<decimal>(() => new decimal(Random.NextDouble())) },
            { typeof(Guid), new Func<Guid>(Guid.NewGuid) },
            { typeof(DateTime), new Func<DateTime>(() => new DateTime(Random.Next())) },
        };

        public static EventInfo[] GetEvents(Type type)
        {
            #if NETCOREAPP1_0
                return type.GetTypeInfo().GetEvents();
            #else
                return type.GetEvents();
            #endif
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Needed to create a generic random function.")]
        public static T GetRandom<T>(bool nullsAllowed)
        {
            var type = typeof(T);

            if (type == typeof(object) || type == typeof(AvroNull))
            {
                return default(T);
            }

            if (type.IsGenerated())
            {
                object avroRecord = Activator.CreateInstance(type);

                /*
                 * the generated type is a class containing one field.
                 * because in feature tests we want to see if Avro type 
                 * (A) can be mapped to C# type (T), to make it possible 
                 * to generate (A) whatever what A is (think of Avro arrays) 
                 * we wrap inside a dummy record Record(A), and check whether 
                 * Record(A) -> Class(T), which is equiavalent to checking A -> T 
                 * because Records map to Classes (proved as a precondition in 
                 * the feature scenario).
                 * It could also be without any fields, these types are used to
                 * demonstrate generation of nesting schemata capability.
                 */
                var property = type.GetProperties().FirstOrDefault(info => info.Name == "AvroField");
                if (property != null)
                {
                    var fixedAttribute = property.GetCustomAttributes(true).OfType<AvroFixedAttribute>().FirstOrDefault();
                    if (fixedAttribute != null)
                    {
                        property.SetValue(avroRecord, FixedBytes(fixedAttribute.Size).ToArray());
                    }
                    else
                    {
                        var propertyType = property.PropertyType;
                        if (property.PropertyType == typeof(object))
                        {
                            propertyType =
                                property.GetCustomAttributes(false).OfType<AvroUnionAttribute>().First().TypeAlternatives.First();
                        }
                        var generateMethod = typeof(Utilities).GetMethod("GetRandom", new[] { typeof(bool) })
                            .MakeGenericMethod(new[] { propertyType });
                        property.SetValue(avroRecord, generateMethod.Invoke(null, new object[] { false }));
                    }
                }
                return (T)avroRecord;
            }

            if (type == typeof(RandomEnumeration))
            {
                var array = Enum.GetValues(typeof(RandomEnumeration));
                return (T)array.GetValue(Random.Next(array.Length));
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var arraySize = Random.Next(100);
                var array = Array.CreateInstance(elementType, arraySize);
                var randomCall = typeof(Utilities).GetMethod("GetRandom").MakeGenericMethod(elementType);
                for (var i = 0; i < arraySize; i++)
                {
                    array.SetValue(randomCall.Invoke(null, new object[] { nullsAllowed }), i);
                }
                return (T)Convert.ChangeType(array, type);
            }

            if (type.GetAllInterfaces().Any(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                || (type.GetTypeInfo().IsInterface && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                var keyType = type.GetGenericArguments()[0];
                var keyRandomCall = typeof(Utilities).GetMethod("GetRandom").MakeGenericMethod(keyType);
                var valueType = type.GetGenericArguments()[1];
                var valueRandomCall = typeof(Utilities).GetMethod("GetRandom").MakeGenericMethod(valueType);
                var dictionaryType = type.GetTypeInfo().IsInterface ? typeof(Dictionary<,>).MakeGenericType(keyType, valueType) : type;
                var dictionary = Activator.CreateInstance(dictionaryType);
                var addMethod = dictionaryType.GetMethod("Add", new[] { keyType, valueType });
                var dictionarySize = Random.Next(100);
                for (var i = 0; i < dictionarySize; i++)
                {
                    addMethod.Invoke(dictionary,
                        new[]
                        {
                            keyRandomCall.Invoke(null, new object[] { nullsAllowed }),
                            valueRandomCall.Invoke(null, new object[] { nullsAllowed })
                        });
                }
                return (T)dictionary;
            }

            if (type != typeof(string))
            {
                Type enumerableType = null;
                foreach (var aType in type.GetInterfaces())
                {
                    if (aType.GetTypeInfo().IsGenericType && aType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        enumerableType = aType.GetGenericArguments()[0];
                    }
                }
                if (enumerableType != null)
                {
                    Type resultType = type.GetTypeInfo().IsInterface ? typeof(List<>).MakeGenericType(enumerableType) : type;
                    var enumerable = Activator.CreateInstance(resultType);
                    var arraySize = Random.Next(100);
                    var enumerableItemTypeCall = typeof(Utilities).GetMethod("GetRandom").MakeGenericMethod(enumerableType);
                    var addMethod = resultType.GetMethod("Add");
                    var temp = Enumerable.Repeat(0, arraySize).Select(i => enumerableItemTypeCall.Invoke(null, new object[] { nullsAllowed })).ToArray();
                    foreach (var value in temp)
                    {
                        addMethod.Invoke(enumerable, new[] { value });
                    }
                    return (T)enumerable;
                }
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                if (nullsAllowed)
                {
                    var shouldBeNull = GetRandom<bool>(false);
                    if (shouldBeNull)
                    {
                        return default(T);
                    }
                }
                var method = typeof(Utilities).GetMethod("GetRandom").MakeGenericMethod(underlyingType);
                return (T)method.Invoke(null, new object[] { nullsAllowed });
            }

            if (type == typeof(Uri))
            {
                return (T)Activator.CreateInstance(typeof(Uri), new object[] { "http://whatever" + GetRandom<string>(nullsAllowed) });
            }

            if (type.GetTypeInfo().IsClass)
            {
                var createMethod = type.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
                if (createMethod != null)
                {
                    if (nullsAllowed)
                    {
                        var shouldBeNull = GetRandom<bool>(false);
                        if (shouldBeNull)
                        {
                            return default(T);
                        }
                    }
                    return (T)createMethod.Invoke(null, new object[] { nullsAllowed });
                }
            }

            object result;
            if (!RandomGenerator.TryGetValue(type, out result))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Do not know how to generate a random value of type '{0}'", type));
            }
            return ((Func<T>)result)();
        }

        public static IEnumerable<byte> FixedBytes(int size)
        {
            for (int i = 0; i < size; i++)
            {
                yield return GetRandom<byte>(false);
            }
        }

        public static bool DictionaryEquals<TK, TV>(IDictionary<TK, TV> first, IDictionary<TK, TV> second)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            if (first.Count != second.Count)
            {
                return false;
            }

            if (first.Keys.Except(second.Keys).Any())
            {
                return false;
            }

            return first.Count(entry => !second[entry.Key].Equals(entry.Value)) == 0;
        }

        public static bool JaggedEquals<T>(T[][] first, T[][] second)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            if (first.Length != second.Length)
            {
                return false;
            }

            for (var i = 0; i < first.Length; i++)
            {
                if (first[i].Length != second[i].Length)
                {
                    return false;
                }

                if (!first[i].SequenceEqual(second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method should be used to compare objects of CodeGen generated types only.
        /// Therefor it does not cover all possible types such as recursive types.
        /// </summary>
        /// <remarks>Generated classes do not have fields therefor we test equality of only properties.</remarks>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Needed for deciding equality.")]
        public static bool GeneratedTypesEquality(object expected, object actual)
        {
            if (expected == null && actual == null)
            {
                return true;
            }

            if (expected == null || actual == null)
            {
                return false;
            }

            if (expected.GetType() != actual.GetType())
            {
                return false;
            }

            var type = expected.GetType();

            if (type.IsPrimitive())
            {
                return expected.Equals(actual);
            }
            if (type.IsArray)
            {
                var firstArray = expected as Array;
                var secondArray = actual as Array;
                if (firstArray.GetLength(0) != secondArray.GetLength(0))
                {
                    return false;
                }
                for (int i = 0; i < firstArray.GetLength(0); i++)
                {
                    if (!GeneratedTypesEquality(firstArray.GetValue(i), secondArray.GetValue(i)))
                    {
                        return false;
                    }
                }
                return true;
            }
            if (type.GetTypeInfo().IsGenericType && typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                var firstList = expected as IList;
                var secondList = actual as IList;
                Debug.Assert(firstList != null, "firstList != null");
                Debug.Assert(secondList != null, "secondList != null");
                if (firstList.Count != secondList.Count)
                {
                    return false;
                }
                return !firstList.Cast<object>().Where((t, i) => !GeneratedTypesEquality(t, secondList[i])).Any();
            }
            if (type.GetTypeInfo().IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                var firstDictionary = expected as IDictionary;
                var secondDictionary = actual as IDictionary;
                Debug.Assert(firstDictionary != null, "firstDictionary != null");
                Debug.Assert(secondDictionary != null, "secondDictionary != null");
                if (firstDictionary.Count != secondDictionary.Count)
                {
                    return false;
                }
                if (
                    firstDictionary.Keys.OfType<string>().Any(key => !secondDictionary.Keys.OfType<string>().Contains(key)))
                {
                    return false;
                }
                return firstDictionary.Keys.Cast<object>().All(key => GeneratedTypesEquality(firstDictionary[key], secondDictionary[key]));
            }
            if (type.GetTypeInfo().IsClass)
            {
                foreach (var property in type.GetProperties())
                {
                    if (!GeneratedTypesEquality(property.GetValue(expected), property.GetValue(actual)))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static void ShouldThrow<T>(Action action)
        {
            try
            {
                action();
                Assert.True(false, "Expected exception of type " + typeof(T) + " but no exception is thrown.");
            }
            catch (Exception e)
            {
                Assert.IsType<T>(e);
            }
        }

        public static void VerifyEquality<T>(T obj1, T obj2, T obj3)
        {
            TestEquals(obj1, obj2, obj3);
            TestEquals(obj1, ClassOfInt.Create(false), ClassOfInt.Create(false));
        }

        private static void TestEquals<T>(T obj1, object obj2, object obj3)
        {
            //reflexivity
            Assert.True(obj1.Equals(obj1));

            //symmetry
            if (obj1.Equals(obj2))
            {
                Assert.True(obj2.Equals(obj1));
                Assert.True(obj1.GetHashCode() == obj2.GetHashCode());
            }
            else
            {
                Assert.False(obj2.Equals(obj1));
            }

            //transitivity
            if (obj1.Equals(obj2))
            {
                Assert.True(obj1.GetHashCode() == obj2.GetHashCode());
                if (obj2.Equals(obj3))
                {
                    Assert.True(obj1.Equals(obj3));
                    Assert.True(obj1.GetHashCode() == obj3.GetHashCode());
                }
                else
                {
                    Assert.False(obj1.Equals(obj3));
                }
            }

            //equality to null
            object nullObject = default(T);
            object anotherNullObject = default(T);

            Assert.False(obj1.Equals(nullObject));
            Assert.True(nullObject == anotherNullObject);
        }

        public static IEnumerable<string> GenerateCode(string jsonSchema, string defaultNamespace, bool namespaceIsForced)
        {
            var sources = new List<string>();

            IEnumerable<TypeSchema> schemas = Utils.CodeGenerator.ResolveCodeGeneratingSchemas(jsonSchema);

            foreach (TypeSchema schema in schemas)
            {
                using (var memoryStream = new MemoryStream())
                {
                    GeneratorCore.Generate(schema, defaultNamespace, namespaceIsForced, memoryStream);
                    memoryStream.Flush();
                    var reader = new StreamReader(memoryStream, new UTF8Encoding());
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    sources.Add(reader.ReadToEnd());
                }
            }

            return sources;
        }

        public static Assembly CompileSources(IEnumerable<string> sources)
        {
            // CSharpCodeProvider is not available on dotnet11, will likely 
            // need to use rosalyn
#if NET46
            using (var csharpCodeProvider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                var compilerParameters = new CompilerParameters { GenerateExecutable = false, GenerateInMemory = true };

                compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
                compilerParameters.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");
                compilerParameters.ReferencedAssemblies.Add("Microsoft.Avro.Core.dll");

                CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(compilerParameters, sources.ToArray());
                Assert.Equal(compilerResults.Errors.Count, 0);

                return compilerResults.CompiledAssembly;
            }
#else
            return null;
#endif
        }

        public static string GetTypeFullName(string type)
        {
            return type.Replace("Object", "System.Object")
                .Replace("Boolean", "System.Boolean")
                .Replace("Byte", "System.Byte")
                .Replace("Int32", "System.Int32")
                .Replace("Int64", "System.Int64")
                .Replace("Single", "System.Single")
                .Replace("Double", "System.Double")
                .Replace("String", "System.String")
                .Replace("IDictionary", "System.Collections.Generic.IDictionary`2")
                .Replace("IList", "System.Collections.Generic.IList`1")
                .Replace("Nullable", "System.Nullable`1")
                .Replace('<', '[')
                .Replace('>', ']')
                .Replace(" ", string.Empty);
        }

#region Type extensions

        public static bool IsGenerated(this Type type)
        {
            return string.IsNullOrEmpty(type.GetTypeInfo().Assembly.Location);
        }

        private static bool IsPrimitive(this Type type)
        {
            return type == typeof(bool)
                   || type == typeof(byte)
                   || type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(float)
                   || type == typeof(double)
                   || type == typeof(string)
                   || type.GetTypeInfo().IsEnum
                   || Nullable.GetUnderlyingType(type) != null;
        }

        public static void ContainersEqual<T1, T2>(IEnumerable<T1> cleft, IEnumerable<T2> cright)
        {
            var l1 = cleft.ToList();
            var l2 = cright.ToList();

            Assert.True(l1.Count() == l2.Count(), "Containers are different lengths");
            l1.Zip<T1, T2, int>(l2,
                (T1 ll, T2 rr) =>
                    {
                        Assert.True(((object)ll).Equals((object)rr), "Container values do not match");
                        return 0;
                    }
                );
        }

#endregion //Type extensions
    }
}
