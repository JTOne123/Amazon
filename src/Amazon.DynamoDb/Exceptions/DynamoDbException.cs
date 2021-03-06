﻿using System;
using System.Collections.Generic;
using System.Text.Json;

using Amazon.Scheduling;

namespace Amazon.DynamoDb
{
    public class DynamoDbException : Exception, IException
    {
        public DynamoDbException(string message)
            : base(message) { }

        public DynamoDbException(string message, string? type)
          : base(message)
        {
            Type = type;
        }

        public DynamoDbException(string message, Exception innerException)
            : base(message, innerException) { }

        public DynamoDbException(string message, List<Exception> exceptions)
            : base(message)
        {
            Exceptions = exceptions;
        }

        public string? Type { get; set; }

        public int StatusCode { get; set; }

        public IReadOnlyList<Exception>? Exceptions { get; }

        public static DynamoDbException Parse(string jsonText)
        {
            using var doc = JsonDocument.Parse(jsonText);

            var json = doc.RootElement;

            string type = "";

            string message = "";

            if (json.TryGetProperty("message", out JsonElement m))
            {
                message = m.GetString();
            }
            else if (json.TryGetProperty("Message", out m))
            {
                message = m.GetString();
            }

            if (json.TryGetProperty("__type", out var typeNode) && typeNode.ValueKind == JsonValueKind.String)
            {
                type = typeNode.GetString();

                int poundIndex = type.IndexOf('#');

                if (poundIndex > -1)
                {
                    type = type.Substring(poundIndex + 1);
                }
            }

            if (type == "ConditionalCheckFailedException")
            {
                return new Conflict(message);
            }

            return new DynamoDbException(message, type);
        }

        public bool IsTransient
        {
            get
            {
                // Client Errors = 4xx (Don't retry)
                // Server Errors = 5xx (Retry)

                switch (Type)
                {
                    case "InternalServerError":
                    case "InternalFailure":
                    case "ProvisionedThroughputExceededException":
                    case "ThrottlingException": return true;
                }

                return false;
            }
        }
    }
}

/*
{
  "__type":"com.amazonaws.dynamodb.v20111205#ProvisionedThroughputExceededException",
  "message":"The level of configured provisioned throughput for the table was exceeded. Consider increasing your provisioning level with the UpdateTable API"
}

{"__type":"com.amazon.coral.service#ExpiredTokenException","message":"The security token included in the request is expired"}

{"__type":"com.amazon.coral.service#SerializationException","Message":"Start of list found where not expected"}
*/
