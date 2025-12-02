Reconsider the refactoring. The direction is good, but it’s still not quite there. You didn’t really follow the SRP principle.
Think through what the exact responsibility of each class should be and organize the functions accordingly. 
There’s still work to be done — if something breaks, it will be harder to debug, and we don’t want incorrect data to slip through.
Review it again to ensure there are no unnecessary computations. Make sure the namespaces are correct, and avoid boilerplate code wherever possible.