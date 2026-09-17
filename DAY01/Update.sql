UPDATE Item
SET name = replace(name, '힐링포션', '힐링 포션')
WHERE name like '힐링포션%';