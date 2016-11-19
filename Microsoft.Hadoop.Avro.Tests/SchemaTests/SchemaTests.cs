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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using Microsoft.Hadoop.Avro.Schema;
    using Xunit;
    
    [Trait("Category","Schema")]
    public sealed class SchemaTests
    {
        [Fact]
        public void SchemaTests_SchemaNameEquality()
        {
            var schemaName = new SchemaName("namespace.name");
            var secondSchemaName = new SchemaName("namespace.name");
            var differentSchemaName = new SchemaName("different.name");
            Utilities.VerifyEquality(schemaName, secondSchemaName, differentSchemaName);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Ctor should throw.")]
        [Fact]
        public void SchemaTests_SchemaNameTryArguments()
        {
            Utilities.ShouldThrow<ArgumentException>(() => new SchemaName(null, "namespace"));
            Utilities.ShouldThrow<ArgumentException>(() => new SchemaName(string.Empty, "namespace"));
            Utilities.ShouldThrow<SerializationException>(() => new SchemaName("name.with.namespace.ending.with.dot.", string.Empty));
            Utilities.ShouldThrow<SerializationException>(() => new SchemaName("invalidnameŠŽŒ", "valid.namespace"));
            Utilities.ShouldThrow<SerializationException>(() => new SchemaName("validName", "invalid.namespaceŠŽŒ"));
        }

        [Fact]
        public void SchemaTests_CreateRecordFieldWithValidPosition()
        {
            var recordSchema = Schema.CreateRecord("SimpleClass", "some.ns");
            Schema.SetFields(
                recordSchema,
                new List<RecordField> { Schema.CreateField("IntField", new IntSchema()), Schema.CreateField("StringField", new StringSchema()) });
            Assert.Equal(2, recordSchema.Fields.Count());
            Assert.IsType(typeof(IntSchema), recordSchema.Fields.First(f => f.Position == 0).TypeSchema);
            Assert.Equal("IntField", recordSchema.Fields.First(f => f.Position == 0).Name);
            Assert.IsType(typeof(StringSchema), recordSchema.Fields.First(f => f.Position == 1).TypeSchema);
            Assert.Equal("StringField", recordSchema.Fields.First(f => f.Position == 1).Name);
        }

        [Fact]
        public void SchemaTests_CreateRecursiveRecordField()
        {
            var recursiveRecordSchema = Schema.CreateRecord("SimpleClass", "some.ns");
            Schema.SetFields(
                recursiveRecordSchema,
                new List<RecordField> { Schema.CreateField("IntField", new IntSchema()), Schema.CreateField("RecursiveField", recursiveRecordSchema) });
            Assert.Equal(2, recursiveRecordSchema.Fields.Count());
            Assert.IsType(typeof(IntSchema), recursiveRecordSchema.Fields.First(f => f.Position == 0).TypeSchema);
            Assert.IsType(recursiveRecordSchema.GetType(), recursiveRecordSchema.Fields.First(f => f.Position == 1).TypeSchema);
            var recursiveField = recursiveRecordSchema.Fields.First(f => f.Position == 1).TypeSchema as RecordSchema;
            Assert.NotNull(recursiveField);
            Assert.Equal(2, recursiveField.Fields.Count());
            Assert.Equal(recursiveField.Name, recursiveRecordSchema.Name);
        }
    }
}
