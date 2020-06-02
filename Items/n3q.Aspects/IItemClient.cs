﻿using n3q.GrainInterfaces;

namespace n3q.Aspects
{
    public interface IItemClient
    {
        string GetId();
        IItem GetItem();
        IItemClient CloneFor(string otherId);
    }
}
