#!/bin/bash
# Runs once each time `docker compose up` starts, after Oracle is healthy.
# On a brand-new database it creates the RESTO user, tables and sample data.
# If RESTO already exists, it does nothing (your data is never touched).

set -euo pipefail

DB="oracle-db:1521/FREEPDB1"

user_count=$(sqlplus -s "system/${ORACLE_PWD}@${DB}" <<'SQL' | tr -d '[:space:]'
SET HEADING OFF FEEDBACK OFF PAGESIZE 0
SELECT COUNT(*) FROM dba_users WHERE username = 'RESTO';
EXIT
SQL
)

if [ "$user_count" = "1" ]; then
  echo "db-init: RESTO schema already exists, nothing to do."
  exit 0
fi

echo "db-init: creating user RESTO..."
sqlplus -s "system/${ORACLE_PWD}@${DB}" <<SQL
WHENEVER SQLERROR EXIT FAILURE
CREATE USER resto IDENTIFIED BY "${RESTO_PWD}"
  DEFAULT TABLESPACE users
  TEMPORARY TABLESPACE temp
  QUOTA UNLIMITED ON users;
GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE, CREATE VIEW,
      CREATE PROCEDURE, CREATE TRIGGER TO resto;
EXIT
SQL

echo "db-init: creating tables..."
sqlplus -s "resto/${RESTO_PWD}@${DB}" < /db/01_schema.sql

echo "db-init: inserting sample menu and tables..."
sqlplus -s "resto/${RESTO_PWD}@${DB}" < /db/02_seed.sql

echo "db-init: done."
