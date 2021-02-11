# Generated by Django 3.1.5 on 2021-02-11 16:57

import certificates.models
from django.db import migrations, models
import django.db.models.deletion
import uuid


class Migration(migrations.Migration):

    initial = True

    dependencies = [
        ('domains', '0001_initial'),
    ]

    operations = [
        migrations.CreateModel(
            name='Certificate',
            fields=[
                ('uuid', models.UUIDField(auto_created=True, default=uuid.uuid4, editable=False, primary_key=True, serialize=False, unique=True, verbose_name='UUID')),
                ('created', models.DateTimeField(auto_now_add=True, verbose_name='date created')),
                ('updated', models.DateTimeField(auto_now=True, verbose_name='date updated')),
                ('certificate', models.FileField(upload_to='', validators=[certificates.models.validate_certificate])),
                ('key', models.FileField(upload_to='', validators=[certificates.models.validate_private_key])),
                ('owner', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, to='domains.domain')),
            ],
            options={
                'abstract': False,
            },
        ),
    ]
