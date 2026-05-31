using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nodify;
using Shell.Models;

namespace Shell.Services
{
    public class VariableManager : ObservableObject
    {
        public ObservableCollection<Variable> Variables { get; } = new ObservableCollection<Variable>();

        public Variable AddVariable(string name, string typeName, VariantValue defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("变量名不能为空", nameof(name));
            if (Variables.Any(v => v.Name == name))
                throw new InvalidOperationException($"变量 '{name}' 已存在");

            var variable = new Variable(name, typeName, defaultValue);
            Variables.Add(variable);
            return variable;
        }

        public bool RemoveVariable(string name)
        {
            var v = Variables.FirstOrDefault(x => x.Name == name);
            if (v != null) { Variables.Remove(v); return true; }
            return false;
        }

        public bool RemoveVariable(Variable variable)
        {
            if (variable == null) return false;
            return Variables.Remove(variable);
        }

        public Variable? GetVariable(string name)
            => Variables.FirstOrDefault(v => v.Name == name);

        public IReadOnlyList<string> GetVariableNames(string? typeFilter = null)
        {
            var query = Variables.AsEnumerable();
            if (!string.IsNullOrEmpty(typeFilter))
                query = query.Where(v => v.TypeName == typeFilter);
            return query.Select(v => v.Name).ToList();
        }

        public void Clear() => Variables.Clear();
    }
}
