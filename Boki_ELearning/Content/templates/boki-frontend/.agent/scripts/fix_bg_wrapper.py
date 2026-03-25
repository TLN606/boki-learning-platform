import os
import re

BASE = r'd:\01_Workspaces\Coding\Websites\boki-frontend'

html_files = [f for f in os.listdir(BASE) if f.endswith('.html')]

# We want to find ALL floating-bg-shape elements, remove them from their current place,
# and inject them all together within a <div class="fixed inset-0 overflow-hidden pointer-events-none z-[-1]"> 
# right after <body ...>

for fname in html_files:
    path = os.path.join(BASE, fname)
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Step 1: Remove existing wrapper if present
    content = content.replace('<div class="absolute inset-x-0 inset-y-0 overflow-hidden z-[-1] pointer-events-none">', '')
    content = content.replace('<!-- Background Ornaments -->', '')
    
    # Extract all floating shapes
    shapes = re.findall(r'<div class="floating-bg-shape[^>]*>.*?</div>', content, flags=re.DOTALL)
    
    if not shapes:
        continue # no shapes in this file

    # Remove all floating shapes from content
    content = re.sub(r'<div class="floating-bg-shape[^>]*>.*?</div>', '', content, flags=re.DOTALL)
    
    # We might have left behind some empty lines or </div> from the broken wrapper in landing.html
    # In landing.html, we previously had:
    # <div class="absolute inset-x-0 inset-y-0 overflow-hidden z-[-1] pointer-events-none">
    # (shapes)
    # </div>
    # Let's clean up any dangling </div> that came exactly after shapes.
    # We'll just carefully inject the new wrapper right after the opening body tag.
    
    wrapper = '\n    <!-- Background Ornaments -->\n    <div class="fixed inset-0 overflow-hidden pointer-events-none z-[-1]">\n        ' + '\n        '.join(shapes) + '\n    </div>'
    
    # Inject after body
    content = re.sub(r'(<body[^>]*>)', r'\1' + wrapper, content, count=1)
    
    # Also clean up duplicate </div> in landing.html (the one we broke earlier)
    # The broken part in landing.html was before <!-- Floating Navigation -->
    content = content.replace('</div>\n\n    <!-- Floating Navigation -->', '\n    <!-- Floating Navigation -->')

    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
        
    print(f"[+] Fixed background shapes in {fname}")

