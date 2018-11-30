﻿using ProtoBuf;

namespace AElf.CLI.Data.Protobuf
{
    [ProtoContract]
    public class Address
    {
        public Address(byte[] val)
        {
            Value = val;
        }
        
        [ProtoMember(1)]
        public byte[] Value { get; set; }
        
        public static implicit operator Address(byte[] value)
        {
            return new Address(value);
        }
    }
    
    [ProtoContract]
    public class Hash
    {
        public Hash(byte[] val)
        {
            Value = val;
        }
        
        [ProtoMember(1)]
        public byte[] Value { get; set; }
        
        public static implicit operator Hash(byte[] value)
        {
            return new Hash(value);
        }
    }
}