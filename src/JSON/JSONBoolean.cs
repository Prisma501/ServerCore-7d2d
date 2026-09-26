using System.Text;

namespace ServerCore.JSON
{
    public class JSONBoolean : JSONValue
    {
        private readonly bool value;

        public JSONBoolean(bool _value)
        {
            value = _value;
        }

        public bool GetBool()
        {
            return value;
        }

        public override void ToString(StringBuilder _stringBuilder, bool _prettyPrint = false, int _currentLevel = 0)
        {
            _stringBuilder.Append(value ? "true" : "false");
        }

        public static JSONBoolean Parse(string _json, ref int _offset)
        {
            if (_json.Substring(_offset, 4).Equals("true"))
            {
                _offset += 4;
                return new JSONBoolean(true);
            }

            if (_json.Substring(_offset, 5).Equals("false"))
            {
                _offset += 5;
                return new JSONBoolean(false);
            }

            throw new MalformedJSONException("No valid boolean found");
        }
    }
}