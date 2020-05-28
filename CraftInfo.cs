namespace spacetrackerb
{
    class CraftInfo
    {
        public string satname { get; set; }
        public int satid { get; set; }
        public CraftInfo(string name, int id)
        {
            this.satname = name;
            this.satid = id;
        }
    }
}
