from odoo import models, fields, api


class TemplateViewerQuestion(models.Model):
    _name = 'template.viewer.question'
    _description = 'Template Question'
    _order = 'question_id'

    template_id = fields.Many2one('template.viewer.template', string='Template', required=True, ondelete='cascade')
    question_id = fields.Integer(string='Question ID', required=True)
    title = fields.Char(string='Question Title', required=True)
    question_type = fields.Selection([
        ('text', 'Text'),
        ('longtext', 'Long Text'),
        ('number', 'Number'),
        ('checkbox', 'Checkbox'),
        ('date', 'Date'),
        ('fileupload', 'File Upload'),
    ], string='Question Type', required=True, default='text')
    
    # Statistics
    answer_count = fields.Integer(string='Answer Count', default=0)
    
    # Number type statistics
    number_average = fields.Float(string='Average')
    number_min = fields.Integer(string='Minimum')
    number_max = fields.Integer(string='Maximum')
    
    # Text type statistics
    most_popular_answers = fields.Text(string='Most Popular Answers')
    
    # Checkbox type statistics
    checkbox_true_count = fields.Integer(string='True Count')
    checkbox_false_count = fields.Integer(string='False Count')
    
    # Date type statistics
    date_min = fields.Datetime(string='Earliest Date')
    date_max = fields.Datetime(string='Latest Date')
    
    # Computed fields
    checkbox_true_percentage = fields.Float(string='True %', compute='_compute_checkbox_percentage', store=True)
    checkbox_false_percentage = fields.Float(string='False %', compute='_compute_checkbox_percentage', store=True)
    
    @api.depends('checkbox_true_count', 'checkbox_false_count', 'answer_count')
    def _compute_checkbox_percentage(self):
        for record in self:
            if record.question_type == 'checkbox' and record.answer_count > 0:
                record.checkbox_true_percentage = (record.checkbox_true_count / record.answer_count) * 100
                record.checkbox_false_percentage = (record.checkbox_false_count / record.answer_count) * 100
            else:
                record.checkbox_true_percentage = 0
                record.checkbox_false_percentage = 0