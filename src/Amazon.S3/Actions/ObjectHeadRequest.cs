﻿using System;
using System.Net.Http;

namespace Amazon.S3
{
    public sealed class ObjectHeadRequest : S3Request
    {
        public ObjectHeadRequest(string host, string bucketName, string key)
            : base(HttpMethod.Head, host, bucketName, key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
        }

        internal void SetCustomerEncryptionKey(in ServerSideEncryptionKey key)
        {
            Headers.Add("x-amz-server-side-encryption-customer-algorithm", key.Algorithm);
            Headers.Add("x-amz-server-side-encryption-customer-key",       Convert.ToBase64String(key.Key));
            Headers.Add("x-amz-server-side-encryption-customer-key-MD5",   Convert.ToBase64String(key.KeyMD5));
        }
    }
}