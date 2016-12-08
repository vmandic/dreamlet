using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.DataAccessLayer.Repository;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class DreamTerm : BaseMongoMetaEntity
    {
        public DreamTerm()
        {
            this.Explanations = new List<DreamExplanation>();
        }
        [BsonRepresentation(BsonType.ObjectId)]
        public string LanguageId { get; set; }
        public Language Language(Func<IRepository<Language>> repo) => repo().GetById(LanguageId);
        public string Term { get; set; }
		public IEnumerable<DreamExplanation> Explanations { get; set; }
    }
}
