-- Run as SYSTEM in FREEPDB1
-- Creates the application schema owner for the restaurant ordering system.
-- Dev-only password: keep it in sync with RESTO_PWD in .env

WHENEVER SQLERROR EXIT FAILURE

CREATE USER IF NOT EXISTS resto IDENTIFIED BY "Resto_2026"
  DEFAULT TABLESPACE users
  TEMPORARY TABLESPACE temp
  QUOTA UNLIMITED ON users;

GRANT CREATE SESSION,
      CREATE TABLE,
      CREATE SEQUENCE,
      CREATE VIEW,
      CREATE PROCEDURE,
      CREATE TRIGGER
TO resto;

PROMPT User RESTO ready.
EXIT
