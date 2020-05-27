#region License
// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md
#endregion

using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Schema.Infrastructure.Validation
{
    internal class ConditionalContext : ContextBase
    {
        public List<ValidationError> Errors;
        public List<JSchema> EvaluatedSchemas;

        private readonly bool _trackEvaluatedSchemas;
        private readonly ConditionalContext _parentConditionalContext;

        public ConditionalContext(Validator validator, ContextBase parentContext, bool trackEvaluatedSchemas)
            : base(validator)
        {
            _parentConditionalContext = parentContext as ConditionalContext;

            // Track evaluated schemas if requested, or the parent context is already tracking.
            _trackEvaluatedSchemas = trackEvaluatedSchemas || (_parentConditionalContext?._trackEvaluatedSchemas ?? false);
        }

        public override void RaiseError(IFormattable message, ErrorType errorType, JSchema schema, object value, IList<ValidationError> childErrors)
        {
            if (Errors == null)
            {
                Errors = new List<ValidationError>();
            }

            Errors.Add(Validator.CreateError(message, errorType, schema, value, childErrors));
        }

        public void TrackEvaluatedSchema(JSchema schema)
        {
            // Optimization to only track evaluated schemas if required, e.g. unevaluatedProperties is set
            if (_trackEvaluatedSchemas)
            {
                if (EvaluatedSchemas == null)
                {
                    EvaluatedSchemas = new List<JSchema>();
                }

                // TODO: Could be smarter about tracking schemas and only store it once
                // rather than each conditional context in the hierarchy.
                EvaluatedSchemas.Add(schema);
                _parentConditionalContext?.TrackEvaluatedSchema(schema);
            }
        }

        public static ConditionalContext Create(ContextBase context, bool trackEvaluatedSchemas)
        {
            return new ConditionalContext(context.Validator, context, trackEvaluatedSchemas);
        }

        public override bool HasErrors => !Errors.IsNullOrEmpty();
    }
}