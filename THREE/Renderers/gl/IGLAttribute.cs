﻿namespace THREE;

public interface IGLAttribute
{
    string Name { get; set; }
    Type Type { get; set; }
    int ItemSize { get; set; }
}