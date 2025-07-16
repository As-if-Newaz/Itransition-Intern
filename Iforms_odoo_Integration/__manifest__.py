{
    'name': 'Template Viewer',
    'version': '1.0.0',
    'category': 'Tools',
    'summary': 'Import and view template aggregated results',
    'description': """
        This module allows importing and viewing template data and aggregated results
        from an external API. It provides a read-only interface to browse templates
        and their question statistics.
    """,
    'author': 'Your Name',
    'depends': ['base', 'web'],
    'data': [
        'security/ir.model.access.csv',
        'data/question_types.xml',
        'views/template_views.xml',
        'views/question_views.xml',
        'wizard/import_wizard_views.xml',
        'views/menu.xml',
    ],
    'assets': {
        'web.assets_backend': [
            'Iforms_odoo_Integration/static/src/css/template_viewer.css',
        ],
    },
    'installable': True,
    'application': True,
    'auto_install': False,
}