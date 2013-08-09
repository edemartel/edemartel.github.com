---
layout: page
title: Delirium Corp
tagline: A variety of ramblings on a variety of subjects
---
{% include JB/setup %}

This is my blog. There are many more like it, but this one is mine. If you want to know more about me, check out the [about](about.html) page. Otherwise, [kick back, relax, take a load off your mind](http://www.youtube.com/watch?v=BxHNztg0X3s).

I realize this is pretty bare bones right now. Bear with me as I learn the ropes about Jekyll.
<hr />

<div class="posts">
  {% for post in site.posts %}
  <h3 style="line-height:32px">
    <small>{{ post.date | date_to_string }} // {{ post.category }}</small><br />
    {{ post.title }}
  </h3>
    <div style="margin-left:1em;">{{ post.excerpt }}
    <a href="{{ BASE_PATH }}{{ post.url }}">&raquo; Read more / comment...</a></div>
  {% endfor %}
</div>


