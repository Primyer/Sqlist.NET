create type user_status
as enum ('Active', 'Passive');

create table if not exists "Users"
(
	"Id" serial not null primary key,
	"Name" name,
	"Email" varchar (125),
	"Phone" varchar (30),
	"EmailIsConfirmed" boolean,
	"CreateDate" timestamp without time zone,
	"Status" user_status default 'Active'
);