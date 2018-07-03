using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WWFramework.Extension;
using WWFramework.Helper;

namespace WWFramework.Util
{
    public class CsvData
    {
        private const string Split = ",";
        private static readonly char[] QuotedChars = { ',', ';' };
        private const string QuoteFormat = "\"{0}\"";
        private static StringBuilder _tempSB;

        private static StringBuilder TempSB
        {
            get
            {
                if (_tempSB == null)
                {
                    _tempSB = new StringBuilder();
                }

                return _tempSB;
            }
        }


        private Dictionary<string, int> _headers;
        private int _headerLine;
        private int HeaderLine
        {
            get
            {
                if (_headers == null)
                {
                    return -1;
                }
                else
                {
                    return _headerLine;
                }
            }
        }
        private List<List<string>> _table;

        public string this[string key, int index]
        {
            get
            {
                if (_headers == null || _table == null)
                {
                    return null;
                }

                var rowIndex = 0;
                if (_headers.TryGetValue(key, out rowIndex))
                {
                    return _table.SafeGetValue(index - HeaderLine + 1).SafeGetValue(rowIndex);
                }

                return null;
            }
        }

        public List<string> this[int index]
        {
            get { return _table.SafeGetValue(index + HeaderLine + 1); }
        }

        public List<string> GetRow(int index)
        {
            return _table.SafeGetValue(index);
        }

        public override string ToString()
        {
            var sb = TempSB;
            sb.Length = 0;

            if (_table != null)
            {
                var firstLine = true;
                foreach (var row in _table)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                    }
                    else
                    {
                        sb.Append(IOHelper.NewLine);
                    }

                    var firstVal = true;
                    foreach (var val in row)
                    {
                        if (firstVal)
                        {
                            firstVal = false;
                        }
                        else
                        {
                            sb.Append(Split);
                        }
                        sb.Append(val);
                    }
                }
            }

            return sb.ToString();
        }

        public void AddObject<T>(T obj, bool skipHeader = false)
        {
            var fields = obj.GetType().GetFields();
            if (!skipHeader && _headers == null)
            {
                AddRow(fields, obj, true);
            }

            AddRow(fields, obj);
        }

        private void AddRow<T>(FieldInfo[] fields, T obj, bool isHeader = false)
        {
            if (_table == null)
            {
                _table = new List<List<string>>();
            }
            var row = new List<string>();

            if (isHeader)
            {
                if (_headers == null)
                {
                    _headers = new Dictionary<string, int>();
                    _headerLine = _table.Count;
                    fields.ForEach((i, info) =>
                    {
                        var name = info.Name;
                        _headers[name] = i;
                        row.Add(name);
                    });
                }
            }
            else
            {
                fields.ForEach((i, info) =>
                {
                    var val = info.GetValue(obj).ToString();
                    if (val.IndexOfAny(QuotedChars) != -1)
                    {
                        val = string.Format(QuoteFormat, val);
                    }
                    row.Add(val);
                });
            }

            _table.Add(row);
        }
    }
}