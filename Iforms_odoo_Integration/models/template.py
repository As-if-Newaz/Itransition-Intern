from odoo import models, fields, api
import requests
import json
from datetime import datetime


class TemplateViewerTemplate(models.Model):
    _name = 'template.viewer.template'
    _description = 'Template'
    _order = 'created_at desc'

    name = fields.Char(string='Title', required=True)
    template_id = fields.Integer(string='Template ID', required=True)
    description = fields.Text(string='Description')
    author_id = fields.Integer(string='Author ID')
    author_name = fields.Char(string='Author Name')
    is_public = fields.Boolean(string='Is Public')
    created_at = fields.Datetime(string='Created At')
    imported_at = fields.Datetime(string='Imported At', default=fields.Datetime.now)
    api_token = fields.Char(string='API Token')
    
    question_ids = fields.One2many('template.viewer.question', 'template_id', string='Questions')
    question_count = fields.Integer(string='Question Count', compute='_compute_question_count', store=True)
    
    @api.depends('question_ids')
    def _compute_question_count(self):
        for record in self:
            record.question_count = len(record.question_ids)
    
    def action_refresh_data(self):
        """Refresh template data from API"""
        if not self.api_token:
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'type': 'warning',
                    'message': 'No API token available for this template',
                }
            }
        
        try:
            # Get base URL from system parameters or use default
            base_url = self.env['ir.config_parameter'].sudo().get_param('template_viewer.api_base_url', 'https://itransition-iforms.runasp.net')
            
            headers = {
                'Authorization': f'Bearer {self.api_token}',
                'Content-Type': 'application/json'
            }
            
            # Import this specific template
            url = f"{base_url}/api/aggregated-results?templateId={self.template_id}"
            response = requests.get(url, headers=headers, timeout=10)
            
            if response.status_code == 200:
                data = response.json()
                self._process_template_data(data)
                return {
                    'type': 'ir.actions.client',
                    'tag': 'display_notification',
                    'params': {
                        'type': 'success',
                        'message': 'Template data refreshed successfully',
                    }
                }
            else:
                return {
                    'type': 'ir.actions.client',
                    'tag': 'display_notification',
                    'params': {
                        'type': 'error',
                        'message': f'Failed to refresh data: {response.status_code}',
                    }
                }
        except Exception as e:
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'type': 'error',
                    'message': f'Error refreshing data: {str(e)}',
                }
            }
    
    def _process_template_data(self, data):
        """Process template data from API response"""
        # Update template info
        self.write({
            'name': data.get('templateTitle', ''),
            'author_id': data.get('authorId'),
            'author_name': data.get('authorName', ''),
        })
        
        # Clear existing questions
        self.question_ids.unlink()
        
        # Create questions
        for question_data in data.get('questions', []):
            mapped_type = self._map_question_type(question_data.get('questionType', ''))
            if not mapped_type:
                continue
            question_vals = {
                'template_id': self.id,
                'question_id': question_data.get('questionId'),
                'title': question_data.get('questionTitle', ''),
                'question_type': mapped_type,
            }
            # For number type
            if mapped_type == 'number':
                question_vals['answer_count'] = question_data.get('answerCount', 0)
                question_vals['number_average'] = question_data.get('numberAverage')
                question_vals['number_min'] = question_data.get('numberMin')
                question_vals['number_max'] = question_data.get('numberMax')
            # For text, longtext, checkbox
            if mapped_type in ['text', 'longtext', 'checkbox']:
                popular_answers = question_data.get('mostPopularAnswers', [])
                if popular_answers:
                    answers_text = []
                    for answer in popular_answers:
                        answers_text.append(f"{answer.get('answer', '')} ({answer.get('count', 0)} times)")
                    question_vals['most_popular_answers'] = '\n'.join(answers_text)
            # For checkbox, optionally set answer_count if present
            if mapped_type == 'checkbox':
                if 'answerCount' in question_data:
                    question_vals['answer_count'] = question_data.get('answerCount', 0)
            # For date and fileupload, do not set any extra fields
            self.env['template.viewer.question'].create(question_vals)
    
    def _map_question_type(self, api_type):
        """Map API question type to Odoo selection"""
        type_mapping = {
            'Text': 'text',
            'LongText': 'longtext', 
            'Number': 'number',
            'Checkbox': 'checkbox',
            'Date': 'date',
            'FileUpload': 'fileupload',
        }
        return type_mapping.get(api_type, 'text')
    
    def _parse_date(self, date_str):
        """Parse date string to datetime object"""
        if not date_str:
            return None
        try:
            # Try different date formats
            for fmt in ('%Y-%m-%dT%H:%M:%S.%f', '%Y-%m-%dT%H:%M:%S', '%Y-%m-%d'):
                try:
                    return datetime.strptime(date_str, fmt)
                except ValueError:
                    continue
        except:
            pass
        return None