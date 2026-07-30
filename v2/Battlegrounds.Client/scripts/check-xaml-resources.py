"""Report {StaticResource}/{DynamicResource} references that resolve to no x:Key.

WPF does not fail the build on a missing resource key: a StaticResource throws at
load time, and a DynamicResource silently resolves to nothing. Both show up only
when the screen in question is opened, which makes them easy to ship. This walks
the XAML and checks every reference against the keys actually defined, so the
whole app can be verified without navigating to each view.

Keys defined anywhere in Themes/ are treated as globally available, since
Themes/Theme.xaml is merged into Application.Resources. Keys defined inside a
view are treated as available to that view only.
"""
import os
import re
import sys
from collections import defaultdict

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Battlegrounds')

KEY_RE = re.compile(r'x:Key="([^"{}]+)"')
REF_RE = re.compile(r'\{(?:Static|Dynamic)Resource\s+([^}\s]+)\s*\}')
# Resource keys also appear as the ResourceKey= form and inside Style BasedOn.
REFKEY_RE = re.compile(r'ResourceKey="?\{?(?:Static|Dynamic)?Resource?\s*([A-Za-z0-9_.]+)')


def xaml_files(*subdirs):
    for sub in subdirs:
        base = os.path.join(ROOT, sub)
        for dirpath, _dirnames, filenames in os.walk(base):
            if 'obj' in dirpath or 'bin' in dirpath:
                continue
            for name in filenames:
                if name.endswith('.xaml'):
                    yield os.path.join(dirpath, name)


def read(path):
    with open(path, encoding='utf-8') as handle:
        return handle.read()


global_keys = set()
for path in xaml_files('Themes'):
    global_keys.update(KEY_RE.findall(read(path)))

# System-supplied and framework keys that are legitimately never declared here.
global_keys.update({'{x:Null}'})

problems = defaultdict(list)
for path in xaml_files('Themes', 'Views'):
    text = read(path)
    local_keys = set(KEY_RE.findall(text))
    available = global_keys | local_keys
    for line_no, line in enumerate(text.splitlines(), start=1):
        for key in REF_RE.findall(line) + REFKEY_RE.findall(line):
            if key.startswith('{'):          # {x:Type ...} implicit styles
                continue
            if key in available:
                continue
            problems[os.path.relpath(path, ROOT)].append((line_no, key))

if not problems:
    print('OK - every resource reference resolves.')
    sys.exit(0)

total = 0
for path in sorted(problems):
    print(path)
    for line_no, key in problems[path]:
        print('  line %-5d %s' % (line_no, key))
        total += 1
print('\n%d unresolved reference(s).' % total)
sys.exit(1)
