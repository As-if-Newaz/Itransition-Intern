from odoo import models, fields, api
import requests
import json
from datetime import datetime


class TemplateViewerImportWizard(models.TransientModel):
    _name = 'template.viewer.import.wizard'
    _description = 'Import Templates Wizard'

    api_token = fields.Char(string='API Token', required=True, help='Enter your API token from the course project')
    api_base_url = fields.Char(string='API Base URL', default='https://itransition-iforms.runasp.net', required=True)
    
    def action_import_templates(self):
        """Import templates from API"""
        if not self.api_token:
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'type': 'warning',
                    'message': 'Please enter an API token',
                }
            }
        
        try:
            # Save base URL to system parameters
            self.env['ir.config_parameter'].sudo().set_param('template_viewer.api_base_url', self.api_base_url)
            
            headers = {
                'Authorization': f'Bearer {self.api_token}',
                'Content-Type': 'application/json'
            }
            
            # First, get list of templates
            templates_url = f"{self.api_base_url}/api/aggregated-results/templates"
            response = requests.get(templates_url, headers=headers, timeout=10)
            
            if response.status_code != 200:
                return {
                    'type': 'ir.actions.client',
                    'tag': 'display_notification',
                    'params': {
                        'type': 'error',
                        'message': f'Failed to fetch templates: {response.status_code}',
                    }
                }
            
            templates_data = response.json()
            imported_count = 0
            
            # Process each template
            for template_info in templates_data.get('templates', []):
                template_id = template_info.get('id')
                
                # Get detailed template data
                detail_url = f"{self.api_base_url}/api/aggregated-results?templateId={template_id}"
                detail_response = requests.get(detail_url, headers=headers, timeout=10)
                
                if detail_response.status_code == 200:
                    detail_data = detail_response.json()
                    
                    # Check if template already exists
                    existing_template = self.env['template.viewer.template'].search([
                        ('template_id', '=', template_id),
                        ('api_token', '=', self.api_token)
                    ], limit=1)
                    
                    if existing_template:
                        # Update existing template
                        existing_template._process_template_data(detail_data)
                        existing_template.imported_at = fields.Datetime.now()
                    else:
                        # Create new template
                        template_vals = {
                            'template_id': template_id,
                            'name': template_info.get('title', ''),
                            'description': template_info.get('description', ''),
                            'is_public': template_info.get('isPublic', False),
                            'created_at': self._parse_date(template_info.get('createdAt')),
                            'api_token': self.api_token,
                            'author_id': template_info.get('createdBy', {}).get('id') if template_info.get('createdBy') else None,
                            'author_name': template_info.get('createdBy', {}).get('userName', '') if template_info.get('createdBy') else '',
                        }
                        
                        template = self.env['template.viewer.template'].create(template_vals)
                        template._process_template_data(detail_data)
                    
                    imported_count += 1
            
            return {
                'type': 'ir.actions.act_window',
                'name': 'Templates',
                'res_model': 'template.viewer.template',
                'view_mode': 'kanban,list,form',
                'context': {
                    'default_api_token': self.api_token,
                },
                'target': 'main',
            }
            
        except requests.exceptions.RequestException as e:
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'type': 'error',
                    'message': f'Connection error: {str(e)}',
                }
            }
        except Exception as e:
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'type': 'error',
                    'message': f'Import error: {str(e)}',
                }
            }
    
    def _parse_date(self, date_str):
        """Parse date string to datetime object"""
        if not date_str:
            return None
        try:
            # Try different date formats
            for fmt in ('%Y-%m-%dT%H:%M:%S.%f', '%Y-%m-%dT%H:%M:%S', '%Y-%m-%d'):
                try:
                    return datetime.strptime(date_str, date_str)
                except ValueError:
                    continue
        except:
            pass
        return None