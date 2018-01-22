using Global.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    // Summary:
    // Defines all NSLDS extension methods
    public static class NsldsExtensions
    {
        // string extension method to limit the length of a string
        public static string Limit(this string s, int length)
        {
            return (s != null && length > 0 && s.Length > length) ? s.Substring(0, length) : s;
        }

        // extension method to simulate the missing "With" keyword
        public static void With<T>(this T item, Action<T> work)
        {
            work(item);
        }

        // extension methods for variables
        public static string CodeName(this string value, string field, List<FahCode> list)
        {
            return list.SingleOrDefault(f => f.FahFieldId == field && f.Code == value)?.Name ?? value;
        }

        public static string CodeDescription(this string value, string field, List<FahCode> list)
        {
            return list.SingleOrDefault(f => f.FahFieldId == field && f.Code == value)?.Name ?? null;
        }

        public static string FieldName(this string value, List<FahField> list)
        {
            return list.SingleOrDefault(f => f.Id == value)?.Name ?? value;
        }

        public static string AlertName(this string value, List<FahAlert> list)
        {
            return list.SingleOrDefault(f => f.Id == value)?.Name ?? value;
        }

        // extension methods for properties
        public static string CodeName<T>(this T instance, Expression<Func<T, object>> field, List<FahCode> list)
        {
            MemberExpression member;
            string fname;

            try
            {
                if (field.Body is MemberExpression) // string fields
                {
                    member = ((MemberExpression)field.Body);
                }
                else if (field.Body is UnaryExpression) // value type fields
                {
                    member = ((UnaryExpression)field.Body).Operand as MemberExpression;
                }
                else { return null; }

                fname = member.Member.Name;
                var fcode = instance.GetType().GetProperty(fname).GetValue(instance).ToString();
                return list.SingleOrDefault(f => f.FahFieldId == fname && f.Code == fcode)?.Name ?? fcode;
            }
            catch
            {
                return null;
            }
        }

        public static string AlertName<T>(this T instance, Expression<Func<T, object>> field, List<FahAlert> list)
        {
            MemberExpression member;
            string falert;

            try
            {
                if (field.Body is MemberExpression) // string fields
                {
                    member = ((MemberExpression)field.Body);
                }
                else if (field.Body is UnaryExpression) // value type fields
                {
                    member = ((UnaryExpression)field.Body).Operand as MemberExpression;
                }
                else { return null; }

                falert = member.Member.Name;
                return list.SingleOrDefault(f => f.Id == falert)?.Name ?? falert;
            }
            catch
            {
                return null;
            }
        }

        public static string AlertDescription<T>(this T instance, Expression<Func<T, object>> field, List<FahAlert> list)
        {
            MemberExpression member;
            string falert;

            try
            {
                if (field.Body is MemberExpression) // string fields
                {
                    member = ((MemberExpression)field.Body);
                }
                else if (field.Body is UnaryExpression) // value type fields
                {
                    member = ((UnaryExpression)field.Body).Operand as MemberExpression;
                }
                else { return null; }

                falert = member.Member.Name;
                return list.SingleOrDefault(f => f.Id == falert)?.Description ?? null;
            }
            catch
            {
                return null;
            }
        }

        public static string FieldName<T>(this T instance, Expression<Func<T, object>> field, List<FahField> list)
        {
            MemberExpression member;
            string fname;

            try
            {
                if (field.Body is MemberExpression) // string fields
                {
                    member = ((MemberExpression)field.Body);
                }
                else if (field.Body is UnaryExpression) // value type fields
                {
                    member = ((UnaryExpression)field.Body).Operand as MemberExpression;
                }
                else { return null; }

                fname = member.Member.Name;
                return list.SingleOrDefault(f => f.Id == fname)?.Name ?? fname;
            }
            catch
            {
                return null;
            }
        }

        public static string FieldDescription<T>(this T instance, Expression<Func<T, object>> field, List<FahField> list)
        {
            MemberExpression member;
            string fname;

            try
            {
                if (field.Body is MemberExpression) // string fields
                {
                    member = ((MemberExpression)field.Body);
                }
                else if (field.Body is UnaryExpression) // value type fields
                {
                    member = ((UnaryExpression)field.Body).Operand as MemberExpression;
                }
                else { return null; }

                fname = member.Member.Name;
                return list.SingleOrDefault(f => f.Id == fname)?.Description ?? null;
            }
            catch
            {
                return null;
            }
        }
    }
}
