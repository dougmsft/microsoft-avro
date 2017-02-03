// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.
namespace Microsoft.Hadoop.Avro.Tests
{
    using System;
    using System.Reflection;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Hadoop.Avro;
    using Microsoft.Hadoop.Avro.Schema;
    using Xunit;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [Trait("Category", "ReflectionSchema")]
    public sealed class ReflectionSchemaTests : IDisposable
    {
        private readonly AvroSerializerSettings settings = new AvroSerializerSettings();

        private ReflectionSchemaBuilder builder;

        public ReflectionSchemaTests()
        {
            this.builder = new ReflectionSchemaBuilder(this.settings);
        }

        public void Dispose()
        {
            this.builder = null;
        }

        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForSimpleFlatClass()
        {
            var recordSchema = this.builder.BuildSchema(typeof(SimpleFlatClass)) as RecordSchema;
            Assert.NotNull(recordSchema);

            Assert.Equal(
                typeof(SimpleFlatClass).GetProperties().Length,
                recordSchema.Fields.Count);
        }

        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForNestedClass()
        {
            var recordSchema = this.builder.BuildSchema(typeof(NestedClass)) as RecordSchema;

            Assert.NotNull(recordSchema);
            Assert.Equal(
                typeof(NestedClass).GetFields().Length,
                recordSchema.Fields.Count);
            Assert.Equal(
                typeof(ClassOfInt).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length,
                (recordSchema.Fields[0].TypeSchema as RecordSchema).Fields.Count);
        }

        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForUnicodeClassName()
        {
            var serializer = AvroSerializer.Create<UnicodeClassNameŠŽŒ>(new AvroSerializerSettings { Resolver = new AvroDataContractResolver(true) });
            var jsonArray = (JArray)JsonConvert.DeserializeObject(serializer.ReaderSchema.ToString());

            var obj = jsonArray[1] as JObject;
            string qualifiedRecordName = obj.GetValue("name", StringComparison.Ordinal).ToString();
            string recordName = qualifiedRecordName.Substring(qualifiedRecordName.LastIndexOf('.') + 1);
            Assert.True(Regex.Match(recordName, @"^[a-zA-Z_]([a-zA-Z0-9_]*)$").Success, @"Avro 1.7.4 spec does not allow unicode characters in names");
        }

        // TODO: think about better expression schemas for tests.
        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForSchemaNullableUsingDataContractResolverWithCSharpNulls()
        {
            var nullableSettings = new AvroSerializerSettings { Resolver = new AvroDataContractResolver(true) };
            RoundTripTestNullableSchema(nullableSettings, true, true, true, true, false, true, true);
        }

        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForSchemaNullableUsingDataContractResolverWithNoNulls()
        {
            var nullableSettings = new AvroSerializerSettings { Resolver = new AvroDataContractResolver(false) };
            RoundTripTestNullableSchema(nullableSettings, false, true, false, true, false, true, false);
        }

        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForSchemaNullableUsingPublicMembersResolverWithCSharpNulls()
        {
            var nullableSettings = new AvroSerializerSettings { Resolver = new AvroPublicMemberContractResolver(true) };
            RoundTripTestNullableSchema(nullableSettings, true, true, true, true, false, true, true);
        }

        [Fact]
        public void ReflectionSchemaBuilder_BuildSchemaForSchemaNullableUsingPublicMembersResolverWithNoNulls()
        {
            var nullableSettings = new AvroSerializerSettings { Resolver = new AvroPublicMemberContractResolver(false) };
            RoundTripTestNullableSchema(nullableSettings, false, true, false, true, false, true, false);
        }

        private static void RoundTripTestNullableSchema(
            AvroSerializerSettings settings,
            bool rootSchemaIsUnion,
            bool nullableValueSchemaNullableIsUnion,
            bool nullableValueSchemaNotNullableIsUnion,
            bool valueSchemaNullableIsUnion,
            bool valueSchemaNotNullableIsUnion,
            bool referenceSchemaNullableIsUnion,
            bool referenceSchemaNotNullableIsUnion)
        {
            var builder = new ReflectionSchemaBuilder(settings);
            var schema = builder.BuildSchema(typeof(ClassWithSchemaNullableField));
            RecordSchema recordSchema = null;

            if (rootSchemaIsUnion)
            {
                var asUnion = schema as UnionSchema;
                Assert.NotNull(asUnion);

                var innerNullSchema = asUnion.Schemas[0] is NullSchema
                ? asUnion.Schemas[0]
                : asUnion.Schemas[1];

                recordSchema = asUnion.Schemas.Single(s => s != innerNullSchema) as RecordSchema;
                Assert.NotNull(recordSchema);
            }
            else
            {
                recordSchema = schema as RecordSchema;
                Assert.NotNull(recordSchema);
            }

            Assert.Equal(typeof(ClassWithSchemaNullableField).GetAllFields().Count(), recordSchema.Fields.Count);

            var nullableValueSchemaNullable = recordSchema.Fields.Single(f => f.Name == "NullableValueNullableSchema");
            ValidateSchema(nullableValueSchemaNullable.TypeSchema, nullableValueSchemaNullableIsUnion, typeof(IntSchema));

            var nullableValueSchemaNotNullable = recordSchema.Fields.Single(f => f.Name == "NullableValueNotNullableSchema");
            ValidateSchema(nullableValueSchemaNotNullable.TypeSchema, nullableValueSchemaNotNullableIsUnion, typeof(IntSchema));

            var valueSchemaNullable = recordSchema.Fields.Single(f => f.Name == "NotNullableValueNullableSchema");
            ValidateSchema(valueSchemaNullable.TypeSchema, valueSchemaNullableIsUnion, typeof(IntSchema));

            var valueSchemaNotNullable = recordSchema.Fields.Single(f => f.Name == "NotNullableValueNotNullableSchema");
            ValidateSchema(valueSchemaNotNullable.TypeSchema, valueSchemaNotNullableIsUnion, typeof(IntSchema));

            var referenceSchemaNullable = recordSchema.Fields.Single(f => f.Name == "ReferenceFieldNullableSchema");
            ValidateSchema(referenceSchemaNullable.TypeSchema, referenceSchemaNullableIsUnion, typeof(RecordSchema));

            var referenceSchemaNotNullable = recordSchema.Fields.Single(f => f.Name == "ReferenceFieldNotNullableSchema");
            ValidateSchema(referenceSchemaNotNullable.TypeSchema, referenceSchemaNotNullableIsUnion, typeof(RecordSchema));
        }

        private static void ValidateSchema(TypeSchema typeSchema, bool isUnion, Type innerSchemaType)
        {
            if (isUnion)
            {
                var asUnion = typeSchema as UnionSchema;
                Assert.NotNull(asUnion);

                var innerNullSchema = asUnion.Schemas[0] is NullSchema
                ? asUnion.Schemas[0]
                : asUnion.Schemas[1];

                var secondSchema = asUnion.Schemas.Single(s => s != innerNullSchema);
                var asNullabelSchema = secondSchema as NullableSchema;
                var innerSchema = asNullabelSchema == null ? secondSchema : asNullabelSchema.ValueSchema;
                Assert.IsType(innerSchemaType, innerSchema);
            }
            else
            {
                var asNullableSchema = typeSchema as NullableSchema;
                var innerType = asNullableSchema == null
                    ? typeSchema
                    : asNullableSchema.ValueSchema;
                Assert.IsType(innerSchemaType, innerType);
            }
        }
    }
}
