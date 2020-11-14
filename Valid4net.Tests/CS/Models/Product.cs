using System;
using System.Collections.Generic;
using System.Text;

namespace Valid4net.Tests.CS.Models
{
    public class Product : Valid4netObject<Product>
    {
        private string _id, _title, _description;

        public Product()
        {
            Rules.Add(nameof(Id), "Id cannot be empty!!", p => !string.IsNullOrEmpty(p.Id));
        }

        public string Id
        {
            get => _id; set
            { SetProperty(ref _id, value); }
        }
        public string Title
        {
            get => _title; set
            { SetProperty(ref _title, value); }
        }

        public string Description
        {
            get => _description; set
            { SetProperty(ref _description, value); }
        }
    }
}
