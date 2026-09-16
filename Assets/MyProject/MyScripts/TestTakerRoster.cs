using System;
using System.Collections.Generic;

// Added 2026-09-16 by Claude Code.
//
// JsonUtility cannot serialize a bare List at the top level of a file, so the
// roster of test takers is wrapped in this. It is the shape of playerData.json.
[Serializable]
public class TestTakerRoster
{
    public List<TestTaker> testTakers = new List<TestTaker>();
}
