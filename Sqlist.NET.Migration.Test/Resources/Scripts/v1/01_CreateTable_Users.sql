create table if not exists "Users"
(
	"Id" serial not null primary key,
	"Name" name,
	"Email" varchar (125),
	"Phone" varchar (30)
);